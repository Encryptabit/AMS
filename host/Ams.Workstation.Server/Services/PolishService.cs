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
    /// <summary>Minimum padding beyond the crossfade zone so handles extend into non-speech audio.</summary>
    private const double HandleGuardSec = 0.030;
    private const double DefaultAuditionContextSec = 0.750;
    /// <summary>Default context window (seconds) before/after splice for context playback preview.</summary>
    private const double DefaultContextPlaybackWindowSec = 2.0;
    private const double MinAuditionClipDurationSec = 0.010;
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
    private readonly EditListService _editListService;

    public PolishService(
        BlazorWorkspace workspace,
        StagingQueueService stagingQueue,
        UndoService undoService,
        PickupMatchingService pickupMatching,
        PreviewBufferService previewBuffer,
        EditListService editListService)
    {
        _workspace = workspace;
        _stagingQueue = stagingQueue;
        _undoService = undoService;
        _pickupMatching = pickupMatching;
        _previewBuffer = previewBuffer;
        _editListService = editListService;
    }

    /// <summary>
    /// Rebuilds the chapter audio from the original treated baseline by re-applying
    /// the given edits back-to-front (descending <see cref="ChapterEdit.BaselineStartSec"/>).
    /// Each edit loads its replacement audio from <see cref="UndoService"/> and applies the
    /// appropriate splice operation. The result is a deterministically-correct chapter
    /// regardless of which edits were reverted.
    /// </summary>
    /// <param name="handle">Active chapter handle providing access to the treated baseline audio.</param>
    /// <param name="editsToApply">The set of edits to re-apply to the baseline.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rebuilt audio buffer with all edits applied.</returns>
    public async Task<AudioBuffer> RebuildChapterAsync(
        ChapterContextHandle handle,
        IReadOnlyList<ChapterEdit> editsToApply,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(handle);
        ArgumentNullException.ThrowIfNull(editsToApply);

        var treatedBuffer = handle.Chapter.Audio.Treated?.Buffer
            ?? throw new InvalidOperationException(
                $"Treated audio buffer is unavailable for chapter '{handle.Chapter.Descriptor.ChapterId}'. " +
                "Cannot rebuild without baseline audio.");

        if (editsToApply.Count == 0)
            return treatedBuffer;

        // Clone the treated buffer data so we can mutate without affecting the cached original.
        // We work on an AudioBuffer reference that gets replaced at each splice step.
        var buffer = treatedBuffer;
        var chapterStem = handle.Chapter.Descriptor.ChapterId;

        // Sort edits by BaselineStartSec DESCENDING — back-to-front application preserves
        // upstream positions because each edit only affects content after itself.
        var sortedEdits = editsToApply
            .OrderByDescending(e => e.BaselineStartSec)
            .ToList();

        foreach (var edit in sortedEdits)
        {
            ct.ThrowIfCancellationRequested();

            switch (edit.Operation)
            {
                case EditOperation.PickupReplace:
                case EditOperation.RoomtoneReplace:
                {
                    var replacement = _undoService.LoadReplacementSegment(chapterStem, edit.Id);
                    if (replacement is null)
                    {
                        Console.WriteLine(
                            $"[RebuildChapter] Replacement segment missing for edit '{edit.Id}' " +
                            $"({edit.Operation}); skipping.");
                        continue;
                    }

                    buffer = AudioSpliceService.ReplaceSegment(
                        buffer,
                        edit.BaselineStartSec,
                        edit.BaselineEndSec,
                        replacement,
                        edit.CrossfadeDurationSec,
                        edit.CrossfadeCurve);
                    break;
                }

                case EditOperation.RoomtoneInsert:
                {
                    var replacement = _undoService.LoadReplacementSegment(chapterStem, edit.Id);
                    if (replacement is null)
                    {
                        Console.WriteLine(
                            $"[RebuildChapter] Replacement segment missing for edit '{edit.Id}' " +
                            $"(RoomtoneInsert); skipping.");
                        continue;
                    }

                    buffer = AudioSpliceService.InsertAtPoint(
                        buffer,
                        edit.BaselineStartSec,
                        replacement,
                        edit.CrossfadeDurationSec,
                        edit.CrossfadeCurve);
                    break;
                }

                case EditOperation.RoomtoneDelete:
                {
                    buffer = AudioSpliceService.DeleteRegion(
                        buffer,
                        edit.BaselineStartSec,
                        edit.BaselineEndSec,
                        edit.CrossfadeDurationSec,
                        edit.CrossfadeCurve);
                    break;
                }

                default:
                    Console.WriteLine(
                        $"[RebuildChapter] Unknown edit operation '{edit.Operation}' for edit '{edit.Id}'; skipping.");
                    break;
            }
        }

        return await Task.FromResult(buffer).ConfigureAwait(false);
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
        var rebasedStartSec = MapBaselineToCurrentTime(chapterStem, originalStartSec);
        var rebasedEndSec = MapBaselineToCurrentTime(chapterStem, originalEndSec);
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
                    prevEnd = MapBaselineToCurrentTime(chapterStem, sentences[idx - 1].Timing?.EndSec);
                if (idx >= 0 && idx < sentences.Count - 1)
                    nextStart = MapBaselineToCurrentTime(chapterStem, sentences[idx + 1].Timing?.StartSec);
            }

            var result = SpliceBoundaryService.RefineBoundariesBreathAware(
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
                var previousStart = MapBaselineToCurrentTime(
                    operationStem,
                    sentences[sentenceIndex - 1].Timing?.StartSec);
                if (previousStart.HasValue)
                {
                    clipStartSec = Math.Min(clipStartSec, previousStart.Value);
                }
            }

            if (sentenceIndex >= 0 && sentenceIndex < sentences.Count - 1)
            {
                var nextEnd = MapBaselineToCurrentTime(
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
    /// Generates a context playback preview that includes surrounding chapter audio spliced
    /// around the pickup replacement. Extracts a window of chapter audio
    /// (±<paramref name="contextWindowSec"/>) around the replacement region, then replaces
    /// the center with the pickup. Stores the result in <see cref="PreviewBufferService"/>.
    /// </summary>
    /// <param name="replacementId">ID of the staged replacement to preview.</param>
    /// <param name="contextWindowSec">Duration of surrounding chapter audio to include (default 2.0s).</param>
    /// <returns>
    /// Preview version and chapter-space clip boundaries:
    /// (PreviewVersion, ChapterStartSec, ChapterEndSec).
    /// </returns>
    public (long PreviewVersion, double ChapterStartSec, double ChapterEndSec) GenerateContextPlaybackPreview(
        string replacementId,
        double contextWindowSec = DefaultContextPlaybackWindowSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);
        contextWindowSec = Math.Max(0.1, contextWindowSec);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        var item = FindStagedItem(replacementId, operationStem);
        var chapterBuffer = GetChapterBuffer(operationHandle);
        var pickupTrimmed = LoadPickupSliceForReplacement(operationHandle, item);

        var chapterDurationSec = (double)chapterBuffer.Length / chapterBuffer.SampleRate;

        // Map baseline coordinates to current (post-edit) timeline positions.
        // GetChapterBuffer returns the current audio (corrected > treated > raw),
        // so we must project baseline positions through the edit list.
        var currentStartSec = MapBaselineToCurrentTime(operationStem, item.OriginalStartSec);
        var currentEndSec = MapBaselineToCurrentTime(operationStem, item.OriginalEndSec);

        // Extract a context window around the replacement region in current-time space
        var contextStartSec = Math.Max(0, currentStartSec - contextWindowSec);
        var contextEndSec = Math.Min(chapterDurationSec, currentEndSec + contextWindowSec);

        if (contextEndSec <= contextStartSec)
        {
            contextEndSec = Math.Min(chapterDurationSec, contextStartSec + MinAuditionClipDurationSec);
        }

        // Trim the context window from the chapter
        var contextClip = AudioProcessor.Trim(
            chapterBuffer,
            TimeSpan.FromSeconds(contextStartSec),
            TimeSpan.FromSeconds(contextEndSec));

        var clipDurationSec = (double)contextClip.Length / contextClip.SampleRate;
        if (clipDurationSec <= 0)
        {
            throw new InvalidOperationException("Unable to generate context playback preview for an empty chapter segment.");
        }

        // Calculate the replacement position relative to the context clip (in current-time space)
        var replaceStartSec = Math.Clamp(currentStartSec - contextStartSec, 0, clipDurationSec);
        var replaceEndSec = Math.Clamp(currentEndSec - contextStartSec, 0, clipDurationSec);
        if (replaceEndSec <= replaceStartSec)
        {
            replaceEndSec = Math.Min(clipDurationSec, replaceStartSec + MinAuditionClipDurationSec);
        }

        // Splice the pickup into the context clip
        var resultBuffer = AudioSpliceService.ReplaceSegment(
            contextClip,
            replaceStartSec,
            replaceEndSec,
            pickupTrimmed,
            item.CrossfadeDurationSec,
            item.CrossfadeCurve);

        _previewBuffer.Set(resultBuffer);
        return (_previewBuffer.Version, contextStartSec, contextEndSec);
    }

    /// <summary>
    /// Applies a staged replacement: backs up the original segment and the pickup audio,
    /// records the edit, then rebuilds the chapter from treated baseline with all edits.
    /// </summary>
    /// <param name="replacementId">ID of the staged replacement to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rebuilt audio buffer and the timing delta (positive = longer, negative = shorter).</returns>
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

            // 5. Save replacement (pickup) audio for rebuild-based revert
            await _undoService.SaveReplacementSegmentAsync(
                item.ChapterStem, replacementId, pickupTrimmed, ct).ConfigureAwait(false);

            // 6. Update status to Applied (also creates ChapterEdit in edit list for timeline projection)
            _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Applied);

            // 7. Rebuild chapter from treated baseline with ALL current edits
            var allEdits = _editListService.GetEdits(operationStem);
            var resultBuffer = await RebuildChapterAsync(operationHandle, allEdits, ct).ConfigureAwait(false);

            // 8. Calculate timing delta
            var timingDelta = pickupDuration - originalDuration;

            // 9. Persist corrected.wav to disk
            PersistCorrectedBuffer(operationHandle, resultBuffer);

            return (resultBuffer, timingDelta);
        }
        finally
        {
            mutationLock.Release();
        }
    }

    /// <summary>
    /// Reverts a previously applied replacement by removing its edit from the edit list,
    /// then rebuilding the chapter from treated baseline with the remaining edits.
    /// If no edits remain, the treated baseline is restored directly.
    /// </summary>
    /// <param name="replacementId">ID of the replacement to revert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rebuilt audio buffer and the negative timing delta.</returns>
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
            // 1. Get undo record for timing delta calculation
            var undoRecord = _undoService.GetUndoRecord(replacementId)
                ?? throw new InvalidOperationException($"No undo record found for replacement '{replacementId}'.");
            if (!string.Equals(undoRecord.ChapterStem, operationStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Replacement '{replacementId}' belongs to chapter '{undoRecord.ChapterStem}', " +
                    $"but active chapter is '{operationStem}'.");
            }

            // 2. Update status to Reverted (removes ChapterEdit from edit list)
            _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Reverted);

            // 3. Get remaining edits and rebuild
            var remainingEdits = _editListService.GetEdits(operationStem);
            AudioBuffer resultBuffer;

            if (remainingEdits.Count > 0)
            {
                // Rebuild from treated baseline with remaining edits
                resultBuffer = await RebuildChapterAsync(operationHandle, remainingEdits, ct).ConfigureAwait(false);
            }
            else
            {
                // No edits remain — restore to original treated audio
                resultBuffer = operationHandle.Chapter.Audio.Treated?.Buffer
                    ?? throw new InvalidOperationException(
                        $"Treated audio buffer is unavailable for chapter '{operationStem}'.");
            }

            // 4. Timing delta is negative of original delta
            var timingDelta = undoRecord.OriginalDurationSec - undoRecord.ReplacementDurationSec;

            // 5. Persist corrected.wav to disk
            PersistCorrectedBuffer(operationHandle, resultBuffer);

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
    /// chapter audio at the specified region. Creates a <see cref="ChapterEdit"/> record
    /// via the unified edit pipeline, saves replacement audio for rebuild, and rebuilds
    /// the chapter from treated baseline with all edits.
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
            // 1. Load current chapter audio (for original segment backup)
            var chapterBuffer = GetChapterBuffer(operationHandle);

            // 2. Load and decode roomtone file
            var roomtoneBuffer = AudioProcessor.Decode(roomtoneFilePath);

            // 3. Determine replacement audio and edit operation type
            AudioBuffer? replacementAudio = null;
            double replacementDurationSec;
            EditOperation editOp;

            switch (request.Operation)
            {
                case RoomtoneOperation.Insert:
                {
                    var insertDuration = request.EndSec - request.StartSec;
                    replacementAudio = insertDuration > 0.001
                        ? AudioSpliceService.GenerateRoomtoneFill(roomtoneBuffer, insertDuration)
                        : roomtoneBuffer;
                    replacementDurationSec = (double)replacementAudio.Length / replacementAudio.SampleRate;
                    editOp = EditOperation.RoomtoneInsert;
                    break;
                }

                case RoomtoneOperation.Replace:
                {
                    var regionDuration = request.EndSec - request.StartSec;
                    replacementAudio = AudioSpliceService.GenerateRoomtoneFill(roomtoneBuffer, regionDuration);
                    replacementDurationSec = regionDuration;
                    editOp = EditOperation.RoomtoneReplace;
                    break;
                }

                case RoomtoneOperation.Delete:
                {
                    replacementDurationSec = 0;
                    editOp = EditOperation.RoomtoneDelete;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Operation));
            }

            // 4. Back up original segment via UndoService
            var undoId = $"roomtone-{Guid.NewGuid():N}";

            _undoService.SaveOriginalSegment(
                operationStem, sentenceId: -1, undoId, chapterBuffer,
                request.StartSec, request.EndSec,
                replacementDurationSec);

            // 5. Save replacement audio for rebuild (null for delete operations)
            if (replacementAudio is not null)
            {
                await _undoService.SaveReplacementSegmentAsync(
                    operationStem, undoId, replacementAudio, ct).ConfigureAwait(false);
            }

            // 6. Create ChapterEdit record for the unified edit pipeline
            var chapterEdit = new ChapterEdit(
                Id: undoId,
                ChapterStem: operationStem,
                Operation: editOp,
                BaselineStartSec: request.StartSec,
                BaselineEndSec: request.EndSec,
                ReplacementDurationSec: replacementDurationSec,
                SentenceId: null,
                ErrorNumber: null,
                PickupAssetId: null,
                CrossfadeDurationSec: request.CrossfadeDurationSec,
                CrossfadeCurve: request.CrossfadeCurve,
                AppliedAtUtc: DateTime.UtcNow);
            _editListService.Add(chapterEdit);

            // 7. Rebuild chapter from treated baseline with ALL current edits
            var allEdits = _editListService.GetEdits(operationStem);
            var resultBuffer = await RebuildChapterAsync(operationHandle, allEdits, ct).ConfigureAwait(false);

            // 8. Persist corrected.wav
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

    /// <summary>
    /// Maps a baseline time to the current (post-edit) timeline using
    /// <see cref="TimelineProjection.BaselineToCurrentTime"/>.
    /// </summary>
    private double? MapBaselineToCurrentTime(string chapterStem, double? baselineTimeSec)
    {
        if (!baselineTimeSec.HasValue)
            return null;

        return MapBaselineToCurrentTime(chapterStem, baselineTimeSec.Value);
    }

    /// <summary>
    /// Maps a baseline time to the current (post-edit) timeline using
    /// <see cref="TimelineProjection.BaselineToCurrentTime"/>.
    /// </summary>
    private double MapBaselineToCurrentTime(string chapterStem, double baselineTimeSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        if (!double.IsFinite(baselineTimeSec))
            return 0;

        var edits = _editListService.GetEdits(chapterStem);
        return TimelineProjection.BaselineToCurrentTime(baselineTimeSec, edits);
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

    /// <summary>
    /// Trims the pickup audio for a replacement with content-aware handle sizing.
    /// The handle zone extends <c>crossfadeDuration + HandleGuardSec</c> beyond
    /// the speech edges so the crossfade fits entirely inside non-speech audio.
    /// </summary>
    private static AudioBuffer LoadPickupSliceForReplacement(
        ChapterContextHandle handle,
        StagedReplacement item)
    {
        var pickupBuffer = handle.Chapter.Book.Audio.LoadPickupByPath(item.PickupSourcePath);
        var pickupDurationSec = (double)pickupBuffer.Length / pickupBuffer.SampleRate;

        // Content-aware handle: crossfade + guard ensures crossfade fits outside speech
        var handlePaddingSec = item.CrossfadeDurationSec + HandleGuardSec;
        var paddedStartSec = Math.Max(0, item.PickupStartSec - handlePaddingSec);
        var paddedEndSec = Math.Min(pickupDurationSec, item.PickupEndSec + handlePaddingSec);

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
