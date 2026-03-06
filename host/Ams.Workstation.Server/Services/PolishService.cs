using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Audio;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Runtime.Chapter;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Orchestrates the full Polish workflow: import pickups, stage replacements,
/// apply with crossfade splice and undo backup, and revert from backup.
/// Ties together <see cref="PickupMatchingService"/>, <see cref="StagingQueueService"/>,
/// <see cref="UndoService"/>, and <see cref="AudioSpliceService"/>.
/// </summary>
public class PolishService
{
    private const double PickupSlicePaddingSec = 0.080;
    private const double DefaultAuditionContextSec = 0.750;
    private const double MinAuditionClipDurationSec = 0.010;
    private const double TimelineMappingEpsilonSec = 0.001;
    private static readonly TreatmentOptions SharedTuningDefaults = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ChapterMutationLocks =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions ArtifactJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BlazorWorkspace _workspace;
    private readonly StagingQueueService _stagingQueue;
    private readonly UndoService _undoService;
    private readonly PickupMatchingService _pickupMatching;
    private readonly PreviewBufferService _previewBuffer;

    public PolishService(
        BlazorWorkspace workspace,
        StagingQueueService stagingQueue,
        UndoService undoService,
        PickupMatchingService pickupMatching,
        PreviewBufferService previewBuffer)
    {
        _workspace = workspace;
        _stagingQueue = stagingQueue;
        _undoService = undoService;
        _pickupMatching = pickupMatching;
        _previewBuffer = previewBuffer;
    }

    /// <summary>
    /// Imports a pickup recording using CRX-driven positional pairing.
    /// Checks cached artifacts for staleness before re-processing.
    /// </summary>
    public async Task<Dictionary<string, List<CrossChapterPickupMatch>>> ImportPickupsCrxAsync(
        string pickupFilePath,
        IReadOnlyList<CrxPickupTarget> crxTargets,
        IProgress<(string Status, double Progress)>? progress = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(crxTargets);

        if (crxTargets.Count == 0)
            return new Dictionary<string, List<CrossChapterPickupMatch>>();

        var fi = new FileInfo(pickupFilePath);
        RegisterBookPickupIfPossible(fi.FullName);
        var crxFingerprint = ComputeCrxFingerprint(crxTargets);

        // Check cached artifacts for freshness
        var cached = TryReadMatchedArtifacts();
        if (cached != null &&
            cached.PickupFilePath == fi.FullName &&
            cached.PickupFileSizeBytes == fi.Length &&
            cached.PickupFileModifiedUtc == fi.LastWriteTimeUtc &&
            cached.CrxTargetsFingerprint == crxFingerprint)
        {
            progress?.Report(("Loaded from cache", 1.0));
            return cached.MatchesByChapter;
        }

        // Run ASR + MFA matching
        progress?.Report(("Running ASR + MFA...", 0.1));

        var matches = await _pickupMatching.MatchPickupCrxAsync(pickupFilePath, crxTargets, ct)
            .ConfigureAwait(false);

        // Group by chapter stem
        progress?.Report(("Distributing matches...", 0.8));

        var result = new Dictionary<string, List<CrossChapterPickupMatch>>(StringComparer.OrdinalIgnoreCase);
        var targetLookup = crxTargets.ToDictionary(t => t.ErrorNumber);

        foreach (var match in matches)
        {
            ct.ThrowIfCancellationRequested();

            if (match.ErrorNumber == null || !targetLookup.TryGetValue(match.ErrorNumber.Value, out var target))
                continue;

            if (!result.TryGetValue(target.ChapterStem, out var chapterMatches))
            {
                chapterMatches = new List<CrossChapterPickupMatch>();
                result[target.ChapterStem] = chapterMatches;
            }

            chapterMatches.Add(new CrossChapterPickupMatch(target.ChapterStem, match));
        }

        // Persist matched artifacts
        var artifacts = new PickupArtifacts(
            fi.FullName,
            DateTime.UtcNow,
            fi.Length,
            fi.LastWriteTimeUtc,
            crxFingerprint,
            new Dictionary<string, List<CrossChapterPickupMatch>>(result));
        WriteMatchedArtifacts(artifacts);

        progress?.Report(("Complete", 1.0));
        return result;
    }

    /// <summary>
    /// Creates a <see cref="StagedReplacement"/> from a match and adds it to the staging queue.
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    /// <param name="match">The pickup match to stage.</param>
    /// <param name="pickupFilePath">Path to the pickup source file.</param>
    /// <param name="originalStartSec">Start time of the original sentence in the chapter audio.</param>
    /// <param name="originalEndSec">End time of the original sentence in the chapter audio.</param>
    /// <param name="crossfadeSec">Crossfade duration in seconds (defaults to treatment splice crossfade).</param>
    /// <param name="curve">Crossfade curve type (defaults to treatment splice curve).</param>
    /// <param name="boundaryOptions">Optional boundary refinement options (defaults to treatment-aligned values).</param>
    /// <returns>The created StagedReplacement record.</returns>
    public StagedReplacement StageReplacement(
        string chapterStem,
        PickupMatch match,
        string pickupFilePath,
        double originalStartSec,
        double originalEndSec,
        double? crossfadeSec = null,
        string? curve = null,
        SpliceBoundaryOptions? boundaryOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(match);
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);

        var effectiveCrossfadeSec = crossfadeSec ?? SharedTuningDefaults.SpliceCrossfadeDurationSec;
        var effectiveCurve = string.IsNullOrWhiteSpace(curve)
            ? SharedTuningDefaults.SpliceCrossfadeCurve
            : curve;
        var effectiveBoundaryOptions = boundaryOptions ?? CreateBoundaryOptionsFromTreatmentDefaults();
        var rebasedStartSec = RebaseTranscriptTimeToCurrentTimeline(chapterStem, originalStartSec);
        var rebasedEndSec = RebaseTranscriptTimeToCurrentTimeline(chapterStem, originalEndSec);
        if (rebasedEndSec <= rebasedStartSec)
        {
            rebasedEndSec = rebasedStartSec + Math.Max(MinAuditionClipDurationSec, originalEndSec - originalStartSec);
        }

        // Refine splice boundaries using silence detection
        var refinedStart = rebasedStartSec;
        var refinedEnd = rebasedEndSec;
        try
        {
            var operationHandle = GetActiveChapterHandleOrThrow();
            var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
            if (!string.Equals(operationStem, chapterStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Cannot stage replacement for chapter '{chapterStem}' while active chapter is '{operationStem}'.");
            }

            operationHandle.Chapter.Book.Audio.RegisterPickup(pickupFilePath);

            var chapterBuffer = GetChapterBuffer(operationHandle);
            var hydrate = GetCurrentHydratedTranscript();
            double? prevEnd = null;
            double? nextStart = null;

            if (hydrate is not null)
            {
                var sentences = hydrate.Sentences;
                int idx = -1;
                for (int i = 0; i < sentences.Count; i++)
                {
                    if (sentences[i].Id == match.SentenceId) { idx = i; break; }
                }

                if (idx > 0)
                    prevEnd = RebaseTranscriptTimeToCurrentTimeline(chapterStem, sentences[idx - 1].Timing?.EndSec);
                if (idx >= 0 && idx < sentences.Count - 1)
                    nextStart = RebaseTranscriptTimeToCurrentTimeline(chapterStem, sentences[idx + 1].Timing?.StartSec);
            }

            var result = SpliceBoundaryService.RefineBoundaries(
                chapterBuffer, rebasedStartSec, rebasedEndSec,
                prevEnd, nextStart, effectiveBoundaryOptions);

            refinedStart = result.RefinedStartSec;
            refinedEnd = result.RefinedEndSec;

            Console.WriteLine(
                $"[BoundaryRefinement] Sentence {match.SentenceId}: " +
                $"transcript {originalStartSec:F3}s-{originalEndSec:F3}s, " +
                $"rebased {rebasedStartSec:F3}s-{rebasedEndSec:F3}s, " +
                $"refined {refinedStart:F3}s-{refinedEnd:F3}s " +
                $"({result.StartMethod}/{result.EndMethod})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BoundaryRefinement] Failed, using original boundaries: {ex.Message}");
        }

        var replacement = new StagedReplacement(
            Id: Guid.NewGuid().ToString("N"),
            ChapterStem: chapterStem,
            SentenceId: match.SentenceId,
            OriginalStartSec: refinedStart,
            OriginalEndSec: refinedEnd,
            PickupSourcePath: pickupFilePath,
            PickupStartSec: match.PickupStartSec,
            PickupEndSec: match.PickupEndSec,
            CrossfadeDurationSec: effectiveCrossfadeSec,
            CrossfadeCurve: effectiveCurve,
            StagedAtUtc: DateTime.UtcNow,
            Status: ReplacementStatus.Staged);

        if (!_stagingQueue.TryStage(replacement, out var validationError))
        {
            throw new InvalidOperationException(
                $"Failed to stage replacement for sentence {match.SentenceId}: {validationError}");
        }

        return replacement;
    }

    private static SpliceBoundaryOptions CreateBoundaryOptionsFromTreatmentDefaults()
    {
        return new SpliceBoundaryOptions
        {
            SilenceThresholdDb = SharedTuningDefaults.SilenceThresholdDb,
            MinSilenceDuration = TimeSpan.FromSeconds(SharedTuningDefaults.MinimumSilenceDuration),
        };
    }

    /// <summary>
    /// Generates a preview by splicing the pickup into the chapter buffer in memory.
    /// The result is cached in <see cref="PreviewBufferService"/> for streaming via the API
    /// but is NOT written to disk.
    /// </summary>
    /// <param name="replacementId">ID of the staged replacement to preview.</param>
    /// <returns>The spliced preview buffer.</returns>
    public AudioBuffer GeneratePreview(string replacementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        var item = FindStagedItem(replacementId, operationStem);
        var chapterBuffer = GetChapterBuffer(operationHandle);

        var pickupTrimmed = LoadPickupSliceForReplacement(operationHandle, item);

        var resultBuffer = AudioSpliceService.ReplaceSegment(
            chapterBuffer,
            item.OriginalStartSec,
            item.OriginalEndSec,
            pickupTrimmed,
            item.CrossfadeDurationSec,
            item.CrossfadeCurve);

        _previewBuffer.Set(resultBuffer);
        return resultBuffer;
    }

    /// <summary>
    /// Generates a staged-audition clip around the replacement boundaries, preferring
    /// previous+current+next sentence coverage when transcript timing is available.
    /// Falls back to fixed context-before/context-after if neighbor sentence timings
    /// cannot be resolved. Stores the clip in <see cref="PreviewBufferService"/>.
    /// </summary>
    /// <param name="replacementId">ID of the staged replacement to audition.</param>
    /// <param name="contextSec">Fallback context duration before/after splice (seconds).</param>
    /// <returns>
    /// Preview version and chapter-space clip boundaries:
    /// (PreviewVersion, ChapterStartSec, ChapterEndSec).
    /// </returns>
    public (long PreviewVersion, double ChapterStartSec, double ChapterEndSec) GenerateAuditionClip(
        string replacementId,
        double contextSec = DefaultAuditionContextSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);
        contextSec = Math.Max(0, contextSec);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        var item = FindStagedItem(replacementId, operationStem);
        var chapterBuffer = GetChapterBuffer(operationHandle);
        var pickupTrimmed = LoadPickupSliceForReplacement(operationHandle, item);

        var chapterDurationSec = (double)chapterBuffer.Length / chapterBuffer.SampleRate;
        var maxClipStartSec = Math.Max(0, chapterDurationSec - MinAuditionClipDurationSec);

        // Fallback context window around the replacement.
        var clipStartSec = Math.Clamp(item.OriginalStartSec - contextSec, 0, maxClipStartSec);
        var clipEndSec = Math.Min(chapterDurationSec, item.OriginalEndSec + contextSec);

        // Preferred window: previous sentence start -> following sentence end.
        var hydrate = GetCurrentHydratedTranscript();
        if (hydrate is not null)
        {
            var sentences = hydrate.Sentences;
            int sentenceIndex = -1;
            for (int i = 0; i < sentences.Count; i++)
            {
                if (sentences[i].Id == item.SentenceId)
                {
                    sentenceIndex = i;
                    break;
                }
            }

            if (sentenceIndex > 0)
            {
                var previousStart = RebaseTranscriptTimeToCurrentTimeline(
                    operationStem,
                    sentences[sentenceIndex - 1].Timing?.StartSec);
                if (previousStart.HasValue)
                {
                    clipStartSec = Math.Min(clipStartSec, previousStart.Value);
                }
            }

            if (sentenceIndex >= 0 && sentenceIndex < sentences.Count - 1)
            {
                var nextEnd = RebaseTranscriptTimeToCurrentTimeline(
                    operationStem,
                    sentences[sentenceIndex + 1].Timing?.EndSec);
                if (nextEnd.HasValue)
                {
                    clipEndSec = Math.Max(clipEndSec, nextEnd.Value);
                }
            }
        }

        clipStartSec = Math.Clamp(clipStartSec, 0, maxClipStartSec);
        clipEndSec = Math.Clamp(clipEndSec, 0, chapterDurationSec);
        if (clipEndSec <= clipStartSec)
        {
            clipEndSec = Math.Min(chapterDurationSec, clipStartSec + MinAuditionClipDurationSec);
        }

        var chapterClip = AudioProcessor.Trim(
            chapterBuffer,
            TimeSpan.FromSeconds(clipStartSec),
            TimeSpan.FromSeconds(clipEndSec));

        var clipDurationSec = (double)chapterClip.Length / chapterClip.SampleRate;
        if (clipDurationSec <= 0)
        {
            throw new InvalidOperationException("Unable to generate audition clip for an empty chapter segment.");
        }

        var replaceStartSec = Math.Clamp(item.OriginalStartSec - clipStartSec, 0, clipDurationSec);
        var replaceEndSec = Math.Clamp(item.OriginalEndSec - clipStartSec, 0, clipDurationSec);
        if (replaceEndSec <= replaceStartSec)
        {
            replaceEndSec = Math.Min(clipDurationSec, replaceStartSec + MinAuditionClipDurationSec);
            if (replaceEndSec <= replaceStartSec)
            {
                replaceStartSec = Math.Max(0, replaceEndSec - MinAuditionClipDurationSec);
            }
        }

        var resultBuffer = AudioSpliceService.ReplaceSegment(
            chapterClip,
            replaceStartSec,
            replaceEndSec,
            pickupTrimmed,
            item.CrossfadeDurationSec,
            item.CrossfadeCurve);

        _previewBuffer.Set(resultBuffer);
        return (_previewBuffer.Version, clipStartSec, clipEndSec);
    }

    /// <summary>
    /// Applies a staged replacement: backs up the original segment, splices in
    /// the pickup audio with crossfade, writes the result to corrected.wav,
    /// and records the timing delta.
    /// </summary>
    /// <param name="replacementId">ID of the staged replacement to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The spliced audio buffer and the timing delta (positive = longer, negative = shorter).</returns>
    public async Task<(AudioBuffer ResultBuffer, double TimingDeltaSec)> ApplyReplacementAsync(
        string replacementId,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        var mutationLock = GetChapterMutationLock(operationStem);
        await mutationLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // 1. Get staged item
            var item = FindStagedItem(replacementId, operationStem);

            EnsureNoActiveOverlapOrThrow(operationStem, item.Id, item.OriginalStartSec, item.OriginalEndSec);

            // 2. Load chapter audio from workspace (corrected > treated > raw)
            var chapterBuffer = GetChapterBuffer(operationHandle);

            // 3. Decode and trim pickup audio
            var pickupTrimmed = LoadPickupSliceForReplacement(operationHandle, item);

            var pickupDuration = (double)pickupTrimmed.Length / pickupTrimmed.SampleRate;
            var originalDuration = item.OriginalEndSec - item.OriginalStartSec;

            // 4. Save original segment via UndoService BEFORE splice
            _undoService.SaveOriginalSegment(
                item.ChapterStem,
                item.SentenceId,
                replacementId,
                chapterBuffer,
                item.OriginalStartSec,
                item.OriginalEndSec,
                pickupDuration);

            // 5. Splice: replace original segment with pickup
            var resultBuffer = AudioSpliceService.ReplaceSegment(
                chapterBuffer,
                item.OriginalStartSec,
                item.OriginalEndSec,
                pickupTrimmed,
                item.CrossfadeDurationSec,
                item.CrossfadeCurve);

            // 6. Calculate timing delta
            var timingDelta = pickupDuration - originalDuration;

            // 7. Persist corrected.wav to disk
            PersistCorrectedBuffer(operationHandle, resultBuffer);

            // 8. Update status to Applied
            _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Applied);

            // 9. Cascade timing delta to downstream items by timeline position.
            _stagingQueue.ShiftDownstream(
                item.ChapterStem,
                item.OriginalEndSec,
                timingDelta,
                replacementId);

            return (resultBuffer, timingDelta);
        }
        finally
        {
            mutationLock.Release();
        }
    }

    /// <summary>
    /// Reverts a previously applied replacement by restoring the original audio segment
    /// from the undo backup, then persists the result to corrected.wav.
    /// </summary>
    /// <param name="replacementId">ID of the replacement to revert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The restored audio buffer and the negative timing delta.</returns>
    public async Task<(AudioBuffer ResultBuffer, double TimingDeltaSec)> RevertReplacementAsync(
        string replacementId,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        var mutationLock = GetChapterMutationLock(operationStem);
        await mutationLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // 1. Get undo record
            var undoRecord = _undoService.GetUndoRecord(replacementId)
                ?? throw new InvalidOperationException($"No undo record found for replacement '{replacementId}'.");
            if (!string.Equals(undoRecord.ChapterStem, operationStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Replacement '{replacementId}' belongs to chapter '{undoRecord.ChapterStem}', " +
                    $"but active chapter is '{operationStem}'.");
            }

            // 2. Load original segment from backup
            var originalSegment = _undoService.LoadOriginalSegment(replacementId)
                ?? throw new InvalidOperationException($"Undo backup file missing for replacement '{replacementId}'.");

            // 3. Get current chapter audio (which has the replacement applied)
            var currentBuffer = GetChapterBuffer(operationHandle);

            // 4. Calculate where the replacement currently sits.
            // Use the queue item's current (shifted) coordinates rather than the stale undo record,
            // because upstream apply/revert may have cascaded timing deltas since this item was applied.
            var queueItem = FindStagedItem(replacementId, operationStem);
            var currentStartSec = queueItem.OriginalStartSec;
            var replacementDuration = undoRecord.ReplacementDurationSec;
            var replacementEndSec = currentStartSec + replacementDuration;

            EnsureNoActiveOverlapOrThrow(operationStem, replacementId, currentStartSec, replacementEndSec);

            // 5. Re-splice: put the original back
            var resultBuffer = AudioSpliceService.ReplaceSegment(
                currentBuffer,
                currentStartSec,
                replacementEndSec,
                originalSegment,
                0.030,
                "tri");

            // 6. Timing delta is negative of original delta
            var timingDelta = undoRecord.OriginalDurationSec - replacementDuration;

            // 7. Persist corrected.wav to disk
            PersistCorrectedBuffer(operationHandle, resultBuffer);

            // 8. Update status to Reverted
            _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Reverted);

            // 9. Cascade the negative delta to downstream staged items.
            _stagingQueue.ShiftDownstream(
                undoRecord.ChapterStem,
                replacementEndSec,
                timingDelta,
                replacementId);

            return (resultBuffer, timingDelta);
        }
        finally
        {
            mutationLock.Release();
        }
    }

    /// <summary>
    /// Returns all staged replacements for a chapter.
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    /// <returns>Read-only list of staged replacements.</returns>
    public IReadOnlyList<StagedReplacement> GetStagedReplacements(string chapterStem)
    {
        return _stagingQueue.GetQueue(chapterStem);
    }

    /// <summary>
    /// Applies a roomtone editing operation (insert, replace, or delete) to the current
    /// chapter audio at the specified region. Backs up the original segment via UndoService,
    /// then persists the result as corrected.wav.
    /// </summary>
    /// <param name="request">The roomtone operation parameters.</param>
    /// <param name="roomtoneFilePath">Path to the roomtone WAV file (used for Insert/Replace).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resulting audio buffer after the operation.</returns>
    public async Task<AudioBuffer> ApplyRoomtoneOperationAsync(
        RoomtoneRequest request,
        string roomtoneFilePath,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(roomtoneFilePath);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        var mutationLock = GetChapterMutationLock(operationStem);
        await mutationLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // 1. Load current chapter audio
            var chapterBuffer = GetChapterBuffer(operationHandle);

            // 2. Load and decode roomtone file
            var roomtoneBuffer = AudioProcessor.Decode(roomtoneFilePath);

            // 3. Apply operation
            AudioBuffer resultBuffer;
            double replacementDurationSec;

            switch (request.Operation)
            {
                case RoomtoneOperation.Insert:
                    // Insert roomtone at a point (use the start position as insertion point)
                    // Duration of insertion = EndSec - StartSec (user drags a region to define insertion length)
                    var insertDuration = request.EndSec - request.StartSec;
                    var insertRoomtone = insertDuration > 0.001
                        ? AudioSpliceService.GenerateRoomtoneFill(roomtoneBuffer, insertDuration)
                        : roomtoneBuffer; // If near-zero width, use roomtone as-is for a brief insert
                    resultBuffer = AudioSpliceService.InsertAtPoint(
                        chapterBuffer, request.StartSec, insertRoomtone,
                        request.CrossfadeDurationSec, request.CrossfadeCurve);
                    replacementDurationSec = (double)insertRoomtone.Length / insertRoomtone.SampleRate;
                    break;

                case RoomtoneOperation.Replace:
                    // Replace selection with looped roomtone of matching duration (Research Pitfall 6)
                    var regionDuration = request.EndSec - request.StartSec;
                    var fillRoomtone = AudioSpliceService.GenerateRoomtoneFill(roomtoneBuffer, regionDuration);
                    resultBuffer = AudioSpliceService.ReplaceSegment(
                        chapterBuffer, request.StartSec, request.EndSec,
                        fillRoomtone, request.CrossfadeDurationSec, request.CrossfadeCurve);
                    replacementDurationSec = regionDuration;
                    break;

                case RoomtoneOperation.Delete:
                    // Delete selection (crossfade join, no replacement)
                    resultBuffer = AudioSpliceService.DeleteRegion(
                        chapterBuffer, request.StartSec, request.EndSec,
                        request.CrossfadeDurationSec, request.CrossfadeCurve);
                    replacementDurationSec = 0;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Operation));
            }

            // 4. Back up original segment via UndoService before persisting
            // Use a synthetic replacement ID for undo tracking (sentenceId = -1 for non-sentence ops)
            var undoId = $"roomtone-{Guid.NewGuid():N}";
            var stem = operationHandle.Chapter.Descriptor.ChapterId;

            _undoService.SaveOriginalSegment(
                stem, sentenceId: -1, undoId, chapterBuffer,
                request.StartSec, request.EndSec,
                replacementDurationSec);

            // 5. Persist corrected.wav
            PersistCorrectedBuffer(operationHandle, resultBuffer);

            return resultBuffer;
        }
        finally
        {
            mutationLock.Release();
        }
    }

    #region CRX Artifact Cache

    private static string ComputeCrxFingerprint(IReadOnlyList<CrxPickupTarget> targets)
    {
        var sb = new StringBuilder();
        foreach (var t in targets.OrderBy(t => t.ErrorNumber))
        {
            sb.Append(t.ErrorNumber).Append('|')
              .Append(t.ChapterStem).Append('|')
              .Append(t.SentenceId).Append('|')
              .Append(t.ShouldBeText).Append('\n');
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private PickupArtifacts? TryReadMatchedArtifacts()
    {
        var workDir = _workspace.WorkingDirectory;
        if (workDir == null) return null;

        var path = Path.Combine(workDir, ".polish", "pickups", "pickups.matched.json");
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PickupArtifacts>(json);
        }
        catch
        {
            return null;
        }
    }

    private void WriteMatchedArtifacts(PickupArtifacts artifacts)
    {
        var workDir = _workspace.WorkingDirectory;
        if (workDir == null) return;

        try
        {
            var dir = Path.Combine(workDir, ".polish", "pickups");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "pickups.matched.json");
            var json = JsonSerializer.Serialize(artifacts, ArtifactJsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write matched artifacts: {Message}", ex.Message);
        }
    }

    #endregion

    #region Private Helpers

    private ChapterContextHandle GetActiveChapterHandleOrThrow()
    {
        return _workspace.CurrentChapterHandle
            ?? throw new InvalidOperationException("No chapter is currently selected.");
    }

    private StagedReplacement FindStagedItem(string replacementId, string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        var chapterQueue = _stagingQueue.GetQueue(chapterStem);
        foreach (var item in chapterQueue)
        {
            if (item.Id == replacementId)
                return item;
        }

        throw new InvalidOperationException(
            $"Replacement '{replacementId}' was not found in active chapter '{chapterStem}'.");
    }

    /// <summary>
    /// Loads the hydrated transcript for the current chapter, if available.
    /// </summary>
    private HydratedTranscript? GetCurrentHydratedTranscript()
    {
        var chapterName = _workspace.CurrentChapterName;
        if (string.IsNullOrEmpty(chapterName))
            return null;

        return _workspace.TryGetHydratedTranscript(chapterName, out var hydrate)
            ? hydrate
            : null;
    }

    private double? RebaseTranscriptTimeToCurrentTimeline(string chapterStem, double? transcriptTimeSec)
    {
        if (!transcriptTimeSec.HasValue)
            return null;

        return RebaseTranscriptTimeToCurrentTimeline(chapterStem, transcriptTimeSec.Value);
    }

    private double RebaseTranscriptTimeToCurrentTimeline(string chapterStem, double transcriptTimeSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        if (!double.IsFinite(transcriptTimeSec))
            return 0;

        var mappedTimeSec = Math.Max(0, transcriptTimeSec);
        var appliedItems = _stagingQueue.GetQueue(chapterStem)
            .Where(item => item.Status == ReplacementStatus.Applied)
            .OrderBy(item => item.OriginalStartSec)
            .ThenBy(item => item.Id);

        foreach (var item in appliedItems)
        {
            var deltaSec = item.PickupDuration() - item.OriginalDuration();
            if (Math.Abs(deltaSec) < TimelineMappingEpsilonSec)
                continue;

            if (item.OriginalEndSec <= mappedTimeSec + TimelineMappingEpsilonSec)
            {
                mappedTimeSec += deltaSec;
            }
        }

        return Math.Max(0, mappedTimeSec);
    }

    /// <summary>
    /// Resolves the best available chapter audio buffer from chapter runtime contexts:
    /// corrected > treated > raw. Buffers are lazy-loaded and cached by AudioBufferManager.
    /// </summary>
    private static AudioBuffer GetChapterBuffer(ChapterContextHandle handle)
    {
        var audio = handle.Chapter.Audio;

        var correctedBuffer = audio.Corrected?.Buffer;
        if (correctedBuffer is not null)
            return correctedBuffer;

        var treatedBuffer = audio.Treated?.Buffer;
        if (treatedBuffer is not null)
            return treatedBuffer;

        var rawBuffer = audio.Raw?.Buffer;
        if (rawBuffer is not null)
            return rawBuffer;

        throw new InvalidOperationException("No audio buffer available for the current chapter.");
    }

    private static AudioBuffer LoadPickupSliceForReplacement(
        ChapterContextHandle handle,
        StagedReplacement item)
    {
        var pickupBuffer = handle.Chapter.Book.Audio.LoadPickupByPath(item.PickupSourcePath);
        var pickupDurationSec = (double)pickupBuffer.Length / pickupBuffer.SampleRate;
        var paddedStartSec = Math.Max(0, item.PickupStartSec - PickupSlicePaddingSec);
        var paddedEndSec = Math.Min(pickupDurationSec, item.PickupEndSec + PickupSlicePaddingSec);

        if (paddedEndSec <= paddedStartSec)
        {
            paddedStartSec = Math.Max(0, item.PickupStartSec);
            paddedEndSec = Math.Min(pickupDurationSec, item.PickupEndSec);
            if (paddedEndSec <= paddedStartSec)
                paddedEndSec = Math.Min(pickupDurationSec, paddedStartSec + 0.010);
        }

        return AudioProcessor.Trim(
            pickupBuffer,
            TimeSpan.FromSeconds(paddedStartSec),
            TimeSpan.FromSeconds(paddedEndSec));
    }

    /// <summary>
    /// Writes the result buffer to {stem}.corrected.wav and flushes the cached
    /// "corrected" AudioBufferContext so it reloads from disk on next access.
    /// Also clears any preview buffer.
    /// Probes the source chapter WAV to preserve its bit depth (e.g. 24-bit audiobook masters).
    /// </summary>
    private void PersistCorrectedBuffer(ChapterContextHandle handle, AudioBuffer buffer)
    {
        var descriptor = handle.Chapter.Descriptor;
        var correctedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.corrected.wav");

        // Resolve bit depth directly from treated-buffer metadata.
        // If treated metadata is unavailable/ambiguous, fail hard.
        var treatedBuffer = handle.Chapter.Audio.Treated?.Buffer
            ?? throw new InvalidOperationException(
                $"Treated audio buffer is unavailable for chapter '{descriptor.ChapterId}'.");
        var sourceBitDepth = ResolveSourceBitDepthOrThrow(treatedBuffer);

        var options = new AudioEncodeOptions(
            TargetSampleRate: buffer.SampleRate,
            TargetBitDepth: sourceBitDepth);
        AudioProcessor.EncodeWav(correctedPath, buffer, options);

        // Flush the cached buffer so next load picks up the new file
        handle.Chapter.Audio.Deallocate("corrected");

        // Clear any in-memory preview
        _previewBuffer.Clear();
    }

    private static int ResolveSourceBitDepthOrThrow(AudioBuffer buffer)
    {
        var metadata = buffer.Metadata
            ?? throw new InvalidOperationException("Audio buffer metadata is missing.");
        var codec = metadata.CodecName?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(codec))
        {
            if (codec.Contains("pcm_s24", StringComparison.Ordinal))
                return 24;
            if (codec.Contains("pcm_s16", StringComparison.Ordinal))
                return 16;
            if (codec.Contains("pcm_s32", StringComparison.Ordinal))
                return 32;
            if (codec.Contains("pcm_f32", StringComparison.Ordinal))
                return 32;
            if (codec.Contains("pcm_f64", StringComparison.Ordinal))
                return 64;
            if (codec.Contains("pcm_s8", StringComparison.Ordinal) ||
                codec.Contains("pcm_u8", StringComparison.Ordinal))
                return 8;
        }

        var sampleFormat = metadata.SourceSampleFormat?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(sampleFormat))
        {
            return sampleFormat switch
            {
                "u8" or "u8p" or "s8" => 8,
                "s16" or "s16p" => 16,
                "s32" or "s32p" => 32,
                "flt" or "fltp" => 32,
                "dbl" or "dblp" => 64,
                "s64" or "s64p" => 64,
                _ => throw new InvalidOperationException(
                    $"Unsupported source sample format '{metadata.SourceSampleFormat}'.")
            };
        }

        throw new InvalidOperationException(
            "Unable to resolve source bit depth from audio metadata. " +
            $"Codec='{metadata.CodecName ?? "<null>"}', SampleFormat='{metadata.SourceSampleFormat ?? "<null>"}'.");
    }

    private void RegisterBookPickupIfPossible(string pickupPath)
    {
        if (!_workspace.IsInitialized)
        {
            return;
        }

        try
        {
            _workspace.Book.Audio.RegisterPickup(pickupPath);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to register pickup in BookAudio cache: {Message}", ex.Message);
        }
    }

    private static SemaphoreSlim GetChapterMutationLock(string chapterStem)
    {
        if (string.IsNullOrWhiteSpace(chapterStem))
            throw new ArgumentException("Chapter stem cannot be null or whitespace.", nameof(chapterStem));
        return ChapterMutationLocks.GetOrAdd(chapterStem, static _ => new SemaphoreSlim(1, 1));
    }

    private void EnsureNoActiveOverlapOrThrow(
        string chapterStem,
        string replacementId,
        double startSec,
        double endSec)
    {
        if (_stagingQueue.TryGetActiveOverlap(
                chapterStem,
                replacementId,
                startSec,
                endSec,
                out var conflict) &&
            conflict is not null)
        {
            throw new InvalidOperationException(
                $"Replacement '{replacementId}' overlaps active replacement '{conflict.Id}' " +
                $"(sentence {conflict.SentenceId}, {conflict.OriginalStartSec:F3}s-{conflict.OriginalEndSec:F3}s).");
        }
    }

    #endregion
}
