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
using Ams.Workstation.Server.Services.Pickups.Edl;
using Ams.Workstation.Server.Services.Pickups.Fit;

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
    private readonly PickupEdlStore _pickupEdlStore;
    private readonly PickupArtifactLedgerStore _pickupArtifactLedgerStore;
    private readonly PickupEdlEngine _pickupEdlEngine;
    private readonly PickupSourceBufferCache _pickupSourceBufferCache;

    public PolishService(
        BlazorWorkspace workspace,
        StagingQueueService stagingQueue,
        UndoService undoService,
        PickupMatchingService pickupMatching,
        PreviewBufferService previewBuffer,
        EditListService editListService,
        PickupEdlStore pickupEdlStore,
        PickupArtifactLedgerStore pickupArtifactLedgerStore,
        PickupEdlEngine pickupEdlEngine,
        PickupSourceBufferCache pickupSourceBufferCache)
    {
        _workspace = workspace;
        _stagingQueue = stagingQueue;
        _undoService = undoService;
        _pickupMatching = pickupMatching;
        _previewBuffer = previewBuffer;
        _editListService = editListService;
        _pickupEdlStore = pickupEdlStore;
        _pickupArtifactLedgerStore = pickupArtifactLedgerStore;
        _pickupEdlEngine = pickupEdlEngine;
        _pickupSourceBufferCache = pickupSourceBufferCache;
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
                            throw new InvalidOperationException(
                                $"[RebuildChapter] Replacement segment missing for edit '{edit.Id}' " +
                                $"({edit.Operation}). Cannot rebuild without all replacement segments. " +
                                $"Remove the edit or restore the missing segment file before retrying.");
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
                            throw new InvalidOperationException(
                                $"[RebuildChapter] Replacement segment missing for edit '{edit.Id}' " +
                                $"(RoomtoneInsert). Cannot rebuild without all replacement segments. " +
                                $"Remove the edit or restore the missing segment file before retrying.");
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
        var sentenceIndexById = BuildSentenceIndexLookup(hydrate);
        var knownSentenceIds = BuildKnownSentenceIdSet(hydrate);

        var replacement = BuildStagedReplacement(
            operationHandle,
            chapterStem,
            match,
            pickupFilePath,
            originalStartSec,
            originalEndSec,
            chapterBuffer,
            hydrate,
            sentenceIndexById,
            crossfadeSec,
            curve,
            boundaryOptions);

        if (!_stagingQueue.TryStage(replacement, out var validationError))
        {
            throw new InvalidOperationException(
                $"Failed to stage replacement for sentence {match.SentenceId}: {validationError}");
        }

        try
        {
            UpsertPickupEdlOperation(
                chapterStem,
                replacement,
                PickupEdlOperationState.Staged,
                knownSentenceIds,
                CancellationToken.None);
        }
        catch
        {
            // Keep queue/EDL authoritative state aligned.
            _stagingQueue.Unstage(replacement.Id);
            throw;
        }

        return replacement;
    }

    /// <summary>
    /// Stages multiple replacements against the active chapter using one shared chapter buffer
    /// and a single queue save, reducing the overhead of Stage All operations.
    /// </summary>
    public (int StagedCount, IReadOnlyList<string> Errors) StageReplacements(
        string chapterStem,
        IReadOnlyList<PickupStageRequest> requests)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(requests);

        if (requests.Count == 0)
        {
            return (0, Array.Empty<string>());
        }

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        if (!string.Equals(operationStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cannot stage replacement for chapter '{chapterStem}' while active chapter is '{operationStem}'.");
        }

        foreach (var pickupPath in requests
                     .Select(r => r.PickupFilePath)
                     .Where(path => !string.IsNullOrWhiteSpace(path))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            operationHandle.Chapter.Book.Audio.RegisterPickup(pickupPath);
        }

        var chapterBuffer = GetChapterBuffer(operationHandle);
        var hydrate = GetCurrentHydratedTranscript();
        var sentenceIndexById = BuildSentenceIndexLookup(hydrate);
        var knownSentenceIds = BuildKnownSentenceIdSet(hydrate);
        var replacements = new List<StagedReplacement>(requests.Count);
        var errors = new List<string>();

        foreach (var request in requests)
        {
            try
            {
                replacements.Add(BuildStagedReplacement(
                    operationHandle,
                    chapterStem,
                    request.Match,
                    request.PickupFilePath,
                    request.OriginalStartSec,
                    request.OriginalEndSec,
                    chapterBuffer,
                    hydrate,
                    sentenceIndexById,
                    request.CrossfadeSec,
                    request.Curve,
                    request.BoundaryOptions));
            }
            catch (Exception ex)
            {
                errors.Add($"Sentence {request.Match.SentenceId}: {ex.Message}");
            }
        }

        var (stagedCount, stageErrors) = _stagingQueue.TryStageMany(replacements);
        if (stageErrors.Count > 0)
        {
            errors.AddRange(stageErrors);
        }

        if (stagedCount == 0)
        {
            return (0, errors);
        }

        var stagedById = _stagingQueue.GetQueue(chapterStem)
            .Where(item => item.Status == ReplacementStatus.Staged)
            .ToDictionary(item => item.Id, StringComparer.Ordinal);

        var rolledBack = 0;
        foreach (var replacement in replacements)
        {
            if (!stagedById.ContainsKey(replacement.Id))
            {
                continue;
            }

            try
            {
                UpsertPickupEdlOperation(
                    chapterStem,
                    replacement,
                    PickupEdlOperationState.Staged,
                    knownSentenceIds,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                rolledBack++;
                _stagingQueue.Unstage(replacement.Id);
                errors.Add($"Sentence {replacement.SentenceId}: EDL stage failed: {ex.Message}");
            }
        }

        return (Math.Max(0, stagedCount - rolledBack), errors);
    }

    private StagedReplacement BuildStagedReplacement(
        ChapterContextHandle operationHandle,
        string chapterStem,
        PickupMatch match,
        string pickupFilePath,
        double originalStartSec,
        double originalEndSec,
        AudioBuffer chapterBuffer,
        HydratedTranscript? hydrate,
        IReadOnlyDictionary<int, int>? sentenceIndexById,
        double? crossfadeSec,
        string? curve,
        SpliceBoundaryOptions? boundaryOptions)
    {
        var effectiveCrossfadeSec = crossfadeSec ?? SharedTuningDefaults.SpliceCrossfadeDurationSec;
        var effectiveCurve = string.IsNullOrWhiteSpace(curve)
            ? SharedTuningDefaults.SpliceCrossfadeCurve
            : curve;
        var effectiveBoundaryOptions = boundaryOptions ?? CreateBoundaryOptionsFromTreatmentDefaults();

        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        if (!string.Equals(operationStem, chapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cannot stage replacement for chapter '{chapterStem}' while active chapter is '{operationStem}'.");
        }

        if (sentenceIndexById is not null && !sentenceIndexById.ContainsKey(match.SentenceId))
        {
            throw new InvalidOperationException(
                $"Cannot stage replacement for unknown sentence '{match.SentenceId}' in chapter '{chapterStem}'.");
        }

        var rebasedStartSec = MapBaselineToCurrentTime(chapterStem, originalStartSec);
        var rebasedEndSec = MapBaselineToCurrentTime(chapterStem, originalEndSec);
        if (rebasedEndSec <= rebasedStartSec)
        {
            rebasedEndSec = rebasedStartSec + Math.Max(MinAuditionClipDurationSec, originalEndSec - originalStartSec);
        }

        var baselineRefinedStart = originalStartSec;
        var baselineRefinedEnd = originalEndSec;
        try
        {
            double? prevEnd = null;
            double? nextStart = null;

            if (hydrate is not null && sentenceIndexById is not null && sentenceIndexById.TryGetValue(match.SentenceId, out var idx))
            {
                var sentences = hydrate.Sentences;
                if (idx > 0)
                {
                    prevEnd = MapBaselineToCurrentTime(chapterStem, sentences[idx - 1].Timing?.EndSec);
                }

                if (idx < sentences.Count - 1)
                {
                    nextStart = MapBaselineToCurrentTime(chapterStem, sentences[idx + 1].Timing?.StartSec);
                }
            }

            var result = SpliceBoundaryService.RefineBoundariesBreathAware(
                chapterBuffer,
                rebasedStartSec,
                rebasedEndSec,
                prevEnd,
                nextStart,
                effectiveBoundaryOptions);

            var refinementDeltaStart = result.RefinedStartSec - rebasedStartSec;
            var refinementDeltaEnd = result.RefinedEndSec - rebasedEndSec;
            baselineRefinedStart = originalStartSec + refinementDeltaStart;
            baselineRefinedEnd = originalEndSec + refinementDeltaEnd;

            if (baselineRefinedEnd <= baselineRefinedStart)
            {
                baselineRefinedStart = originalStartSec;
                baselineRefinedEnd = originalEndSec;
            }

            Console.WriteLine(
                $"[BoundaryRefinement] Sentence {match.SentenceId}: " +
                $"transcript(baseline) {originalStartSec:F3}s-{originalEndSec:F3}s, " +
                $"current {rebasedStartSec:F3}s-{rebasedEndSec:F3}s, " +
                $"refined(current) {result.RefinedStartSec:F3}s-{result.RefinedEndSec:F3}s, " +
                $"stored(baseline) {baselineRefinedStart:F3}s-{baselineRefinedEnd:F3}s " +
                $"({result.StartMethod}/{result.EndMethod})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BoundaryRefinement] Failed, using original boundaries: {ex.Message}");
        }

        return new StagedReplacement(
            Id: Guid.NewGuid().ToString("N"),
            ChapterStem: chapterStem,
            SentenceId: match.SentenceId,
            OriginalStartSec: baselineRefinedStart,
            OriginalEndSec: baselineRefinedEnd,
            PickupSourcePath: pickupFilePath,
            PickupStartSec: match.PickupStartSec,
            PickupEndSec: match.PickupEndSec,
            CrossfadeDurationSec: effectiveCrossfadeSec,
            CrossfadeCurve: effectiveCurve,
            StagedAtUtc: DateTime.UtcNow,
            Status: ReplacementStatus.Staged);
    }

    private static IReadOnlyDictionary<int, int>? BuildSentenceIndexLookup(HydratedTranscript? hydrate)
    {
        if (hydrate is null)
        {
            return null;
        }

        var lookup = new Dictionary<int, int>(hydrate.Sentences.Count);
        for (var i = 0; i < hydrate.Sentences.Count; i++)
        {
            lookup[hydrate.Sentences[i].Id] = i;
        }

        return lookup;
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
        var source = ResolveSourceReferenceForReplacement(operationStem, item);

        var pickupTrimmed = LoadPickupSliceForReplacement(operationStem, item, source, CancellationToken.None);

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
        var source = ResolveSourceReferenceForReplacement(operationStem, item);
        var pickupTrimmed = LoadPickupSliceForReplacement(operationStem, item, source, CancellationToken.None);

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

        var chapterClip = chapterBuffer.SliceClamped(
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
        var source = ResolveSourceReferenceForReplacement(operationStem, item);
        var pickupTrimmed = LoadPickupSliceForReplacement(operationStem, item, source, CancellationToken.None);

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

        // Slice the context window from the chapter
        var contextClip = chapterBuffer.SliceClamped(
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

        StagedReplacement? item = null;
        IReadOnlySet<int>? knownSentenceIds = null;
        PickupEdlSourceReference? source = null;
        var transitionedToApplied = false;

        try
        {
            item = FindStagedItem(replacementId, operationStem);
            EnsureReplacementStatusForApply(item, operationStem);
            knownSentenceIds = BuildKnownSentenceIdSet(GetCurrentHydratedTranscript());

            EnsureNoActiveOverlapOrThrow(operationStem, item.Id, item.OriginalStartSec, item.OriginalEndSec);

            var (seededDocument, seededSource) = EnsurePickupEdlOperationSeeded(
                operationStem,
                item,
                knownSentenceIds,
                ct);
            source = seededSource;

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.CommitAttempt,
                phase: "apply",
                edlRevision: seededDocument.Revision,
                queueStatus: ReplacementStatus.Staged,
                edlState: PickupEdlOperationState.Staged,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                failureReason: null,
                ct);

            var chapterBuffer = GetChapterBuffer(operationHandle);
            var pickupTrimmed = LoadPickupSliceForReplacement(operationStem, item, source, ct);

            var pickupDuration = (double)pickupTrimmed.Length / pickupTrimmed.SampleRate;
            var originalDuration = item.OriginalEndSec - item.OriginalStartSec;

            _undoService.SaveOriginalSegment(
                item.ChapterStem,
                item.SentenceId,
                replacementId,
                chapterBuffer,
                item.OriginalStartSec,
                item.OriginalEndSec,
                pickupDuration);

            await _undoService.SaveReplacementSegmentAsync(
                item.ChapterStem,
                replacementId,
                pickupTrimmed,
                ct).ConfigureAwait(false);

            var appliedDocument = TransitionPickupEdlOperationState(
                operationStem,
                source,
                replacementId,
                PickupEdlOperationState.Applied,
                ct);
            transitionedToApplied = true;

            if (!_stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Applied, syncEditList: false))
            {
                throw new InvalidOperationException(
                    $"Pickup commit failed to update queue status for chapter '{operationStem}', op '{replacementId}'.");
            }

            SyncPickupEditsFromEdl(operationStem, appliedDocument);

            var allEdits = _editListService.GetEdits(operationStem);
            var resultBuffer = await RebuildChapterAsync(operationHandle, allEdits, ct).ConfigureAwait(false);
            var timingDelta = pickupDuration - originalDuration;

            PersistCorrectedBuffer(operationHandle, resultBuffer);

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.CommitSuccess,
                phase: "apply",
                edlRevision: appliedDocument.Revision,
                queueStatus: ReplacementStatus.Applied,
                edlState: PickupEdlOperationState.Applied,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                failureReason: null,
                CancellationToken.None);

            return (resultBuffer, timingDelta);
        }
        catch (OperationCanceledException cancelEx)
        {
            var rollbackVerdict = PickupArtifactLedgerRollbackVerdict.NotAttempted;
            var terminalQueueStatus = ReplacementStatus.Failed;
            var terminalEdlState = PickupEdlOperationState.Failed;

            if (transitionedToApplied && item is not null && source is not null)
            {
                var rollbackSucceeded = TryRollbackPickupTransition(
                    operationStem,
                    source,
                    replacementId,
                    rollbackEdlState: PickupEdlOperationState.Staged,
                    rollbackQueueStatus: ReplacementStatus.Staged,
                    phase: "apply-cancel",
                    trigger: cancelEx);

                rollbackVerdict = rollbackSucceeded
                    ? PickupArtifactLedgerRollbackVerdict.Succeeded
                    : PickupArtifactLedgerRollbackVerdict.Failed;
                terminalQueueStatus = rollbackSucceeded ? ReplacementStatus.Staged : ReplacementStatus.Failed;
                terminalEdlState = rollbackSucceeded ? PickupEdlOperationState.Staged : PickupEdlOperationState.Failed;

                if (!rollbackSucceeded)
                {
                    PersistFailedPickupState(operationStem, item, knownSentenceIds, cancelEx, CancellationToken.None);
                }
            }
            else if (item is not null)
            {
                terminalQueueStatus = item.Status;
                terminalEdlState = PickupEdlOperationState.Staged;
            }

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.CommitCancelled,
                phase: "apply-cancel",
                edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                queueStatus: terminalQueueStatus,
                edlState: terminalEdlState,
                rollbackVerdict: rollbackVerdict,
                failureReason: cancelEx.Message,
                CancellationToken.None);

            throw;
        }
        catch (Exception ex)
        {
            var rollbackVerdict = PickupArtifactLedgerRollbackVerdict.NotAttempted;
            var terminalQueueStatus = ReplacementStatus.Failed;
            var terminalEdlState = PickupEdlOperationState.Failed;

            if (transitionedToApplied && item is not null && source is not null)
            {
                var rollbackSucceeded = TryRollbackPickupTransition(
                    operationStem,
                    source,
                    replacementId,
                    rollbackEdlState: PickupEdlOperationState.Staged,
                    rollbackQueueStatus: ReplacementStatus.Staged,
                    phase: "apply-failure",
                    trigger: ex);

                rollbackVerdict = rollbackSucceeded
                    ? PickupArtifactLedgerRollbackVerdict.Succeeded
                    : PickupArtifactLedgerRollbackVerdict.Failed;
                terminalQueueStatus = rollbackSucceeded ? ReplacementStatus.Staged : ReplacementStatus.Failed;
                terminalEdlState = rollbackSucceeded ? PickupEdlOperationState.Staged : PickupEdlOperationState.Failed;

                if (!rollbackSucceeded)
                {
                    PersistFailedPickupState(operationStem, item, knownSentenceIds, ex, CancellationToken.None);
                }
            }
            else if (item is not null)
            {
                PersistFailedPickupState(operationStem, item, knownSentenceIds, ex, ct);
                terminalQueueStatus = ReplacementStatus.Failed;
                terminalEdlState = PickupEdlOperationState.Failed;
            }

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.CommitFailure,
                phase: "apply-failure",
                edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                queueStatus: terminalQueueStatus,
                edlState: terminalEdlState,
                rollbackVerdict: rollbackVerdict,
                failureReason: ex.Message,
                CancellationToken.None);

            throw;
        }
        finally
        {
            mutationLock.Release();
        }
    }

    public Task<FitReplacementPreviewResult> GenerateFitPreviewAsync(
        PickupFitPlanDocument fitPlan,
        PickupFitPlanItem fitItem,
        string? roomtoneFilePath,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(fitPlan);
        ArgumentNullException.ThrowIfNull(fitItem);
        ct.ThrowIfCancellationRequested();

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        if (!string.Equals(operationStem, fitPlan.ChapterStem, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(operationStem, fitItem.Target.ChapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Pickup Fit preview rejected: active chapter='{operationStem}', fitPlan='{fitPlan.ChapterStem}', itemTarget='{fitItem.Target.ChapterStem}'.");
        }

        var source = BuildPickupEdlSourceReference(fitPlan.Source);
        var pickupSource = _pickupSourceBufferCache.GetSourceBuffer(source, operationStem, fitItem.ReplacementId, ct);
        var render = RenderFitReplacement(operationHandle, fitItem, pickupSource, roomtoneFilePath);
        var chapterBuffer = GetChapterBuffer(operationHandle);
        var chapterDurationSec = (double)chapterBuffer.Length / chapterBuffer.SampleRate;
        var maxClipStartSec = Math.Max(0, chapterDurationSec - MinAuditionClipDurationSec);

        var currentOuterStartSec = MapBaselineToCurrentTime(operationStem, fitItem.OuterRange.StartSec);
        var currentOuterEndSec = MapBaselineToCurrentTime(operationStem, fitItem.OuterRange.EndSec);
        var clipStartSec = Math.Clamp(currentOuterStartSec, 0, maxClipStartSec);
        var clipEndSec = Math.Clamp(currentOuterEndSec, 0, chapterDurationSec);

        var hydrate = GetCurrentHydratedTranscript();
        if (hydrate is not null)
        {
            var sentences = hydrate.Sentences;
            var sentenceIndex = -1;
            for (var i = 0; i < sentences.Count; i++)
            {
                if (sentences[i].Id == fitItem.Target.SentenceId)
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

        var contextClip = chapterBuffer.SliceClamped(
            TimeSpan.FromSeconds(clipStartSec),
            TimeSpan.FromSeconds(clipEndSec));
        var clipDurationSec = (double)contextClip.Length / contextClip.SampleRate;
        if (clipDurationSec <= 0)
        {
            throw new InvalidOperationException("Unable to generate Fit preview for an empty chapter context segment.");
        }

        var replaceStartSec = Math.Clamp(currentOuterStartSec - clipStartSec, 0, clipDurationSec);
        var replaceEndSec = Math.Clamp(currentOuterEndSec - clipStartSec, 0, clipDurationSec);
        if (replaceEndSec <= replaceStartSec)
        {
            replaceEndSec = Math.Min(clipDurationSec, replaceStartSec + MinAuditionClipDurationSec);
        }

        var resultBuffer = AudioSpliceService.ReplaceSegment(
            contextClip,
            replaceStartSec,
            replaceEndSec,
            render.Buffer,
            render.EffectiveCrossfadeDurationSec,
            render.CrossfadeCurve);

        _previewBuffer.Set(resultBuffer);
        if (_previewBuffer.Version > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Pickup Fit preview version '{_previewBuffer.Version}' exceeds supported persisted range.");
        }

        var renderedDurationSec = (double)resultBuffer.Length / resultBuffer.SampleRate;
        return Task.FromResult(new FitReplacementPreviewResult(
            ResultBuffer: resultBuffer,
            PreviewVersion: (int)_previewBuffer.Version,
            RenderedDurationSec: renderedDurationSec,
            ChapterStartSec: clipStartSec,
            ChapterEndSec: clipEndSec));
    }

    public async Task<FitReplacementCommitResult> CommitFitReplacementAsync(
        PickupFitPlanDocument fitPlan,
        PickupFitPlanItem fitItem,
        string? roomtoneFilePath,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(fitPlan);
        ArgumentNullException.ThrowIfNull(fitItem);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        if (!string.Equals(operationStem, fitPlan.ChapterStem, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(operationStem, fitItem.Target.ChapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Pickup Fit commit rejected: active chapter='{operationStem}', fitPlan='{fitPlan.ChapterStem}', itemTarget='{fitItem.Target.ChapterStem}'.");
        }

        var mutationLock = GetChapterMutationLock(operationStem);
        await mutationLock.WaitAsync(ct).ConfigureAwait(false);

        StagedReplacement? item = null;
        IReadOnlySet<int>? knownSentenceIds = null;
        PickupEdlSourceReference? source = null;
        var transitionedToApplied = false;

        try
        {
            ct.ThrowIfCancellationRequested();
            EnsureFitItemReadyForCommit(fitPlan, fitItem);
            knownSentenceIds = BuildKnownSentenceIdSet(GetCurrentHydratedTranscript());

            source = BuildPickupEdlSourceReference(fitPlan.Source);
            var pickupSource = _pickupSourceBufferCache.GetSourceBuffer(source, operationStem, fitItem.ReplacementId, ct);
            var render = RenderFitReplacement(operationHandle, fitItem, pickupSource, roomtoneFilePath);

            item = BuildFitStagedReplacement(fitPlan, fitItem, render);
            EnsureNoActiveOverlapOrThrow(operationStem, item.Id, item.OriginalStartSec, item.OriginalEndSec);
            EnsureFitReplacementQueued(operationStem, item);

            var (seededDocument, seededSource) = EnsurePickupEdlOperationSeeded(
                operationStem,
                item,
                knownSentenceIds,
                ct);
            source = seededSource;

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: item.Id,
                transition: PickupArtifactLedgerTransitions.CommitAttempt,
                phase: "fit-apply",
                edlRevision: seededDocument.Revision,
                queueStatus: ReplacementStatus.Staged,
                edlState: PickupEdlOperationState.Staged,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                failureReason: null,
                ct);

            var chapterBuffer = GetChapterBuffer(operationHandle);
            var originalDuration = item.OriginalEndSec - item.OriginalStartSec;

            _undoService.SaveOriginalSegment(
                item.ChapterStem,
                item.SentenceId,
                item.Id,
                chapterBuffer,
                item.OriginalStartSec,
                item.OriginalEndSec,
                render.RenderedDurationSec);

            await _undoService.SaveReplacementSegmentAsync(
                item.ChapterStem,
                item.Id,
                render.Buffer,
                ct).ConfigureAwait(false);

            var appliedDocument = TransitionPickupEdlOperationState(
                operationStem,
                source,
                item.Id,
                PickupEdlOperationState.Applied,
                ct);
            transitionedToApplied = true;

            if (!_stagingQueue.UpdateStatus(item.Id, ReplacementStatus.Applied, syncEditList: false))
            {
                throw new InvalidOperationException(
                    $"Pickup Fit commit failed to update queue status for chapter '{operationStem}', op '{item.Id}'.");
            }

            SyncPickupEditsFromEdl(operationStem, appliedDocument);

            var allEdits = _editListService.GetEdits(operationStem);
            var resultBuffer = await RebuildChapterAsync(operationHandle, allEdits, ct).ConfigureAwait(false);
            var timingDelta = render.RenderedDurationSec - originalDuration;

            PersistCorrectedBuffer(operationHandle, resultBuffer);

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: item.Id,
                transition: PickupArtifactLedgerTransitions.CommitSuccess,
                phase: "fit-apply",
                edlRevision: appliedDocument.Revision,
                queueStatus: ReplacementStatus.Applied,
                edlState: PickupEdlOperationState.Applied,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                failureReason: null,
                CancellationToken.None);

            return new FitReplacementCommitResult(
                ResultBuffer: resultBuffer,
                TimingDeltaSec: timingDelta,
                OperationId: item.Id,
                RenderedReplacementDurationSec: render.RenderedDurationSec,
                EdlRevision: appliedDocument.Revision);
        }
        catch (OperationCanceledException cancelEx)
        {
            var rollbackVerdict = PickupArtifactLedgerRollbackVerdict.NotAttempted;
            var terminalQueueStatus = item?.Status ?? ReplacementStatus.Failed;
            var terminalEdlState = PickupEdlOperationState.Staged;

            if (transitionedToApplied && item is not null && source is not null)
            {
                var rollbackSucceeded = TryRollbackPickupTransition(
                    operationStem,
                    source,
                    item.Id,
                    rollbackEdlState: PickupEdlOperationState.Staged,
                    rollbackQueueStatus: ReplacementStatus.Staged,
                    phase: "fit-apply-cancel",
                    trigger: cancelEx);

                rollbackVerdict = rollbackSucceeded
                    ? PickupArtifactLedgerRollbackVerdict.Succeeded
                    : PickupArtifactLedgerRollbackVerdict.Failed;
                terminalQueueStatus = rollbackSucceeded ? ReplacementStatus.Staged : ReplacementStatus.Failed;
                terminalEdlState = rollbackSucceeded ? PickupEdlOperationState.Staged : PickupEdlOperationState.Failed;

                if (!rollbackSucceeded)
                {
                    PersistFailedPickupState(operationStem, item, knownSentenceIds, cancelEx, CancellationToken.None);
                }
            }

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: fitItem.ReplacementId,
                transition: PickupArtifactLedgerTransitions.CommitCancelled,
                phase: "fit-apply-cancel",
                edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                queueStatus: terminalQueueStatus,
                edlState: terminalEdlState,
                rollbackVerdict: rollbackVerdict,
                failureReason: cancelEx.Message,
                CancellationToken.None);

            throw;
        }
        catch (Exception ex)
        {
            var rollbackVerdict = PickupArtifactLedgerRollbackVerdict.NotAttempted;
            var terminalQueueStatus = ReplacementStatus.Failed;
            var terminalEdlState = PickupEdlOperationState.Failed;

            if (transitionedToApplied && item is not null && source is not null)
            {
                var rollbackSucceeded = TryRollbackPickupTransition(
                    operationStem,
                    source,
                    item.Id,
                    rollbackEdlState: PickupEdlOperationState.Staged,
                    rollbackQueueStatus: ReplacementStatus.Staged,
                    phase: "fit-apply-failure",
                    trigger: ex);

                rollbackVerdict = rollbackSucceeded
                    ? PickupArtifactLedgerRollbackVerdict.Succeeded
                    : PickupArtifactLedgerRollbackVerdict.Failed;
                terminalQueueStatus = rollbackSucceeded ? ReplacementStatus.Staged : ReplacementStatus.Failed;
                terminalEdlState = rollbackSucceeded ? PickupEdlOperationState.Staged : PickupEdlOperationState.Failed;

                if (!rollbackSucceeded)
                {
                    PersistFailedPickupState(operationStem, item, knownSentenceIds, ex, CancellationToken.None);
                }
            }
            else if (item is not null)
            {
                PersistFailedPickupState(operationStem, item, knownSentenceIds, ex, CancellationToken.None);
            }

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: fitItem.ReplacementId,
                transition: PickupArtifactLedgerTransitions.CommitFailure,
                phase: "fit-apply-failure",
                edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                queueStatus: terminalQueueStatus,
                edlState: terminalEdlState,
                rollbackVerdict: rollbackVerdict,
                failureReason: ex.Message,
                CancellationToken.None);

            throw;
        }
        finally
        {
            mutationLock.Release();
        }
    }

    public async Task<(AudioBuffer? ResultBuffer, int AppliedCount)> ApplyReplacementsAsync(
        IReadOnlyList<string> replacementIds,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(replacementIds);

        if (replacementIds.Count == 0)
            return (null, 0);

        var operationHandle = GetActiveChapterHandleOrThrow();
        var operationStem = operationHandle.Chapter.Descriptor.ChapterId;
        var mutationLock = GetChapterMutationLock(operationStem);
        await mutationLock.WaitAsync(ct).ConfigureAwait(false);

        IReadOnlySet<int>? knownSentenceIds = null;
        var transitionedToApplied = new List<(StagedReplacement Item, PickupEdlSourceReference Source)>();

        try
        {
            var chapterBuffer = GetChapterBuffer(operationHandle);
            knownSentenceIds = BuildKnownSentenceIdSet(GetCurrentHydratedTranscript());
            var appliedCount = 0;
            PickupEdlDocument? latestDocument = _pickupEdlStore.TryRead(operationStem, ct);

            foreach (var replacementId in replacementIds)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var item = FindStagedItem(replacementId, operationStem);
                    EnsureReplacementStatusForApply(item, operationStem);
                    EnsureNoActiveOverlapOrThrow(operationStem, item.Id, item.OriginalStartSec, item.OriginalEndSec);

                    var (seededDocument, source) = EnsurePickupEdlOperationSeeded(
                        operationStem,
                        item,
                        knownSentenceIds,
                        ct);
                    latestDocument = seededDocument;

                    TryAppendPickupArtifactLedgerEntry(
                        chapterStem: operationStem,
                        operationId: replacementId,
                        transition: PickupArtifactLedgerTransitions.CommitAttempt,
                        phase: "batch-apply",
                        edlRevision: seededDocument.Revision,
                        queueStatus: ReplacementStatus.Staged,
                        edlState: PickupEdlOperationState.Staged,
                        rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                        failureReason: null,
                        ct);

                    var pickupTrimmed = LoadPickupSliceForReplacement(operationStem, item, source, ct);
                    var pickupDuration = (double)pickupTrimmed.Length / pickupTrimmed.SampleRate;

                    _undoService.SaveOriginalSegment(
                        item.ChapterStem,
                        item.SentenceId,
                        replacementId,
                        chapterBuffer,
                        item.OriginalStartSec,
                        item.OriginalEndSec,
                        pickupDuration);

                    await _undoService.SaveReplacementSegmentAsync(
                        item.ChapterStem,
                        replacementId,
                        pickupTrimmed,
                        ct).ConfigureAwait(false);

                    latestDocument = TransitionPickupEdlOperationState(
                        operationStem,
                        source,
                        replacementId,
                        PickupEdlOperationState.Applied,
                        ct);

                    if (!_stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Applied, syncEditList: false))
                    {
                        throw new InvalidOperationException(
                            $"Batch pickup commit failed to update queue status for chapter '{operationStem}', op '{replacementId}'.");
                    }

                    transitionedToApplied.Add((item, source));
                    appliedCount++;

                    TryAppendPickupArtifactLedgerEntry(
                        chapterStem: operationStem,
                        operationId: replacementId,
                        transition: PickupArtifactLedgerTransitions.CommitSuccess,
                        phase: "batch-apply",
                        edlRevision: latestDocument.Revision,
                        queueStatus: ReplacementStatus.Applied,
                        edlState: PickupEdlOperationState.Applied,
                        rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                        failureReason: null,
                        CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var failedItem = TryFindStagedItem(replacementId, operationStem);
                    if (failedItem is not null)
                    {
                        PersistFailedPickupState(operationStem, failedItem, knownSentenceIds, ex, ct);
                    }

                    TryAppendPickupArtifactLedgerEntry(
                        chapterStem: operationStem,
                        operationId: replacementId,
                        transition: PickupArtifactLedgerTransitions.CommitFailure,
                        phase: "batch-apply-item",
                        edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                        queueStatus: failedItem?.Status ?? ReplacementStatus.Failed,
                        edlState: PickupEdlOperationState.Failed,
                        rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotAttempted,
                        failureReason: ex.Message,
                        CancellationToken.None);

                    Log.Warn(
                        "Batch pickup commit skipped replacement {ReplacementId} in chapter {ChapterStem}: {Message}",
                        replacementId,
                        operationStem,
                        ex.Message);
                }
            }

            if (appliedCount == 0)
            {
                return (null, 0);
            }

            latestDocument ??= _pickupEdlStore.TryRead(operationStem, ct);
            if (latestDocument is not null)
            {
                SyncPickupEditsFromEdl(operationStem, latestDocument);
            }

            var allEdits = _editListService.GetEdits(operationStem);
            var resultBuffer = await RebuildChapterAsync(operationHandle, allEdits, ct).ConfigureAwait(false);
            PersistCorrectedBuffer(operationHandle, resultBuffer);

            return (resultBuffer, appliedCount);
        }
        catch (OperationCanceledException cancelEx)
        {
            if (transitionedToApplied.Count > 0)
            {
                var rollbackSucceeded = TryRollbackBatchPickupTransitions(
                    operationStem,
                    transitionedToApplied,
                    rollbackEdlState: PickupEdlOperationState.Staged,
                    rollbackQueueStatus: ReplacementStatus.Staged,
                    phase: "batch-apply-cancel",
                    trigger: cancelEx);

                if (!rollbackSucceeded)
                {
                    foreach (var (item, _) in transitionedToApplied)
                    {
                        PersistFailedPickupState(operationStem, item, knownSentenceIds, cancelEx, CancellationToken.None);
                    }
                }

                foreach (var (item, _) in transitionedToApplied)
                {
                    TryAppendPickupArtifactLedgerEntry(
                        chapterStem: operationStem,
                        operationId: item.Id,
                        transition: PickupArtifactLedgerTransitions.CommitCancelled,
                        phase: "batch-apply-cancel",
                        edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                        queueStatus: rollbackSucceeded ? ReplacementStatus.Staged : ReplacementStatus.Failed,
                        edlState: rollbackSucceeded ? PickupEdlOperationState.Staged : PickupEdlOperationState.Failed,
                        rollbackVerdict: rollbackSucceeded
                            ? PickupArtifactLedgerRollbackVerdict.Succeeded
                            : PickupArtifactLedgerRollbackVerdict.Failed,
                        failureReason: cancelEx.Message,
                        CancellationToken.None);
                }
            }

            throw;
        }
        catch (Exception ex)
        {
            if (transitionedToApplied.Count > 0)
            {
                var rollbackSucceeded = TryRollbackBatchPickupTransitions(
                    operationStem,
                    transitionedToApplied,
                    rollbackEdlState: PickupEdlOperationState.Staged,
                    rollbackQueueStatus: ReplacementStatus.Staged,
                    phase: "batch-apply-failure",
                    trigger: ex);

                if (!rollbackSucceeded)
                {
                    foreach (var (item, _) in transitionedToApplied)
                    {
                        PersistFailedPickupState(operationStem, item, knownSentenceIds, ex, CancellationToken.None);
                    }
                }

                foreach (var (item, _) in transitionedToApplied)
                {
                    TryAppendPickupArtifactLedgerEntry(
                        chapterStem: operationStem,
                        operationId: item.Id,
                        transition: PickupArtifactLedgerTransitions.CommitFailure,
                        phase: "batch-apply-failure",
                        edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                        queueStatus: rollbackSucceeded ? ReplacementStatus.Staged : ReplacementStatus.Failed,
                        edlState: rollbackSucceeded ? PickupEdlOperationState.Staged : PickupEdlOperationState.Failed,
                        rollbackVerdict: rollbackSucceeded
                            ? PickupArtifactLedgerRollbackVerdict.Succeeded
                            : PickupArtifactLedgerRollbackVerdict.Failed,
                        failureReason: ex.Message,
                        CancellationToken.None);
                }
            }

            throw;
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

        StagedReplacement? item = null;
        IReadOnlySet<int>? knownSentenceIds = null;
        PickupEdlSourceReference? source = null;
        PickupEdlOperation? operationBeforeRevert = null;
        var transitionedToReverted = false;

        try
        {
            var undoRecord = _undoService.GetUndoRecord(replacementId)
                ?? throw new InvalidOperationException(
                    $"Pickup revert rejected for chapter '{operationStem}', op '{replacementId}': undo record is missing or malformed.");
            if (!string.Equals(undoRecord.ChapterStem, operationStem, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup revert rejected for op '{replacementId}': undo chapter='{undoRecord.ChapterStem}', active chapter='{operationStem}'.");
            }

            item = FindStagedItem(replacementId, operationStem);
            EnsureReplacementStatusForRevert(item, operationStem);
            knownSentenceIds = BuildKnownSentenceIdSet(GetCurrentHydratedTranscript());

            var current = _pickupEdlStore.TryRead(operationStem, ct)
                ?? throw new InvalidOperationException(
                    $"Pickup revert rejected for chapter '{operationStem}', op '{replacementId}': pickup EDL document not found.");

            operationBeforeRevert = _pickupEdlEngine.TryGetOperation(current, replacementId)
                ?? throw new InvalidOperationException(
                    $"Pickup revert rejected for chapter '{operationStem}', op '{replacementId}': operation is missing from pickup EDL.");

            if (operationBeforeRevert.State != PickupEdlOperationState.Applied)
            {
                throw new InvalidOperationException(
                    $"Pickup revert rejected for chapter '{operationStem}', op '{replacementId}': EDL state is '{operationBeforeRevert.State}', expected 'Applied'.");
            }

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.RevertAttempt,
                phase: "revert",
                edlRevision: current.Revision,
                queueStatus: ReplacementStatus.Applied,
                edlState: PickupEdlOperationState.Applied,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                failureReason: null,
                ct);

            source = current.Source;
            var revertedDocument = TransitionPickupEdlOperationState(
                operationStem,
                source,
                replacementId,
                PickupEdlOperationState.Reverted,
                ct);
            transitionedToReverted = true;

            if (!_stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Reverted, syncEditList: false))
            {
                throw new InvalidOperationException(
                    $"Pickup revert failed to update queue status for chapter '{operationStem}', op '{replacementId}'.");
            }

            SyncPickupEditsFromEdl(operationStem, revertedDocument);

            var remainingEdits = _editListService.GetEdits(operationStem);
            AudioBuffer resultBuffer;

            if (remainingEdits.Count > 0)
            {
                resultBuffer = await RebuildChapterAsync(operationHandle, remainingEdits, ct).ConfigureAwait(false);
            }
            else
            {
                resultBuffer = operationHandle.Chapter.Audio.Treated?.Buffer
                    ?? throw new InvalidOperationException(
                        $"Treated audio buffer is unavailable for chapter '{operationStem}'.");
            }

            var timingDelta = undoRecord.OriginalDurationSec - undoRecord.ReplacementDurationSec;
            PersistCorrectedBuffer(operationHandle, resultBuffer);

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.RevertSuccess,
                phase: "revert",
                edlRevision: revertedDocument.Revision,
                queueStatus: ReplacementStatus.Reverted,
                edlState: PickupEdlOperationState.Reverted,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.NotRequired,
                failureReason: null,
                CancellationToken.None);

            return (resultBuffer, timingDelta);
        }
        catch (OperationCanceledException cancelEx)
        {
            var rollbackVerdict = PickupArtifactLedgerRollbackVerdict.NotAttempted;
            var terminalQueueStatus = item?.Status ?? ReplacementStatus.Failed;
            var terminalEdlState = operationBeforeRevert?.State ?? PickupEdlOperationState.Failed;

            if (transitionedToReverted && item is not null && source is not null && operationBeforeRevert is not null)
            {
                var rollbackSucceeded = TryRestorePickupOperationSnapshot(
                    operationStem,
                    source,
                    operationBeforeRevert,
                    rollbackQueueStatus: ReplacementStatus.Applied,
                    phase: "revert-cancel",
                    trigger: cancelEx);

                rollbackVerdict = rollbackSucceeded
                    ? PickupArtifactLedgerRollbackVerdict.Succeeded
                    : PickupArtifactLedgerRollbackVerdict.Failed;
                terminalQueueStatus = rollbackSucceeded ? ReplacementStatus.Applied : ReplacementStatus.Failed;
                terminalEdlState = rollbackSucceeded ? PickupEdlOperationState.Applied : PickupEdlOperationState.Failed;

                if (!rollbackSucceeded)
                {
                    PersistFailedPickupState(operationStem, item, knownSentenceIds, cancelEx, CancellationToken.None);
                }
            }

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.RevertCancelled,
                phase: "revert-cancel",
                edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                queueStatus: terminalQueueStatus,
                edlState: terminalEdlState,
                rollbackVerdict: rollbackVerdict,
                failureReason: cancelEx.Message,
                CancellationToken.None);

            throw;
        }
        catch (Exception ex)
        {
            var rollbackVerdict = PickupArtifactLedgerRollbackVerdict.NotAttempted;
            var terminalQueueStatus = item?.Status ?? ReplacementStatus.Failed;
            var terminalEdlState = operationBeforeRevert?.State ?? PickupEdlOperationState.Failed;

            if (transitionedToReverted && item is not null && source is not null && operationBeforeRevert is not null)
            {
                var rollbackSucceeded = TryRestorePickupOperationSnapshot(
                    operationStem,
                    source,
                    operationBeforeRevert,
                    rollbackQueueStatus: ReplacementStatus.Applied,
                    phase: "revert-failure",
                    trigger: ex);

                rollbackVerdict = rollbackSucceeded
                    ? PickupArtifactLedgerRollbackVerdict.Succeeded
                    : PickupArtifactLedgerRollbackVerdict.Failed;
                terminalQueueStatus = rollbackSucceeded ? ReplacementStatus.Applied : ReplacementStatus.Failed;
                terminalEdlState = rollbackSucceeded ? PickupEdlOperationState.Applied : PickupEdlOperationState.Failed;

                if (!rollbackSucceeded)
                {
                    PersistFailedPickupState(operationStem, item, knownSentenceIds, ex, CancellationToken.None);
                }
            }

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: operationStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.RevertFailure,
                phase: "revert-failure",
                edlRevision: ResolveCurrentPickupEdlRevision(operationStem, CancellationToken.None),
                queueStatus: terminalQueueStatus,
                edlState: terminalEdlState,
                rollbackVerdict: rollbackVerdict,
                failureReason: ex.Message,
                CancellationToken.None);

            throw;
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

    private StagedReplacement? TryFindStagedItem(string replacementId, string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        var chapterQueue = _stagingQueue.GetQueue(chapterStem);
        foreach (var item in chapterQueue)
        {
            if (item.Id == replacementId)
            {
                return item;
            }
        }

        return null;
    }

    private static IReadOnlySet<int>? BuildKnownSentenceIdSet(HydratedTranscript? hydrate)
    {
        if (hydrate is null)
        {
            return null;
        }

        var set = new HashSet<int>();
        foreach (var sentence in hydrate.Sentences)
        {
            set.Add(sentence.Id);
        }

        return set;
    }

    private static void EnsureFitItemReadyForCommit(PickupFitPlanDocument fitPlan, PickupFitPlanItem fitItem)
    {
        if (!string.Equals(fitItem.Target.ChapterStem, fitPlan.ChapterStem, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Pickup Fit commit rejected for item '{fitItem.FitItemId}': item chapter '{fitItem.Target.ChapterStem}' does not match plan chapter '{fitPlan.ChapterStem}'.");
        }

        if (!fitItem.Acceptance.IsAccepted || fitItem.PreviewEvidence is null)
        {
            throw new InvalidOperationException(
                $"Pickup Fit commit rejected for item '{fitItem.FitItemId}': accepted preview evidence is required before commit.");
        }

        if (!fitItem.PreviewEvidence.MatchesPickTruth(fitPlan.PickMapRevision, fitPlan.PickAssignmentsFingerprint))
        {
            throw new InvalidOperationException(
                $"Pickup Fit commit rejected for item '{fitItem.FitItemId}': preview evidence is stale for pick revision/fingerprint.");
        }

        if (fitItem.Acceptance.AcceptedPreviewVersion != fitItem.PreviewEvidence.PreviewVersion)
        {
            throw new InvalidOperationException(
                $"Pickup Fit commit rejected for item '{fitItem.FitItemId}': accepted preview version does not match preview evidence.");
        }
    }

    private static PickupEdlSourceReference BuildPickupEdlSourceReference(PickupPickMapSourceReference source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new PickupEdlSourceReference(
            path: source.Path,
            fingerprint: source.Fingerprint,
            fileSizeBytes: source.FileSizeBytes,
            modifiedAtUtc: source.ModifiedAtUtc);
    }

    private PickupFitAudioRenderResult RenderFitReplacement(
        ChapterContextHandle operationHandle,
        PickupFitPlanItem fitItem,
        AudioBuffer pickupSource,
        string? roomtoneFilePath)
    {
        try
        {
            return PickupFitAudioRenderer.Render(fitItem, pickupSource, roomtoneSource: null);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("roomtone is required", StringComparison.OrdinalIgnoreCase))
        {
            var roomtone = ResolveFitRoomtoneBuffer(operationHandle, roomtoneFilePath, fitItem.FitItemId)
                ?? throw new InvalidOperationException(
                    $"Pickup Fit commit rejected for item '{fitItem.FitItemId}': roomtone fill is required but no roomtone source is available.",
                    ex);
            return PickupFitAudioRenderer.Render(fitItem, pickupSource, roomtone);
        }
    }

    private static AudioBuffer? ResolveFitRoomtoneBuffer(
        ChapterContextHandle operationHandle,
        string? roomtoneFilePath,
        string fitItemId)
    {
        if (!string.IsNullOrWhiteSpace(roomtoneFilePath))
        {
            var file = new FileInfo(roomtoneFilePath.Trim());
            if (!file.Exists)
            {
                throw new FileNotFoundException(
                    $"Pickup Fit commit rejected for item '{fitItemId}': roomtone file is missing at '{file.FullName}'.",
                    file.FullName);
            }

            return AudioProcessor.Decode(file.FullName);
        }

        return operationHandle.Chapter.Book.Audio.Roomtone;
    }

    private static StagedReplacement BuildFitStagedReplacement(
        PickupFitPlanDocument fitPlan,
        PickupFitPlanItem fitItem,
        PickupFitAudioRenderResult render)
    {
        var metadata = new PickupEdlFitMetadata(
            fitItemId: fitItem.FitItemId,
            pickAssignmentId: fitItem.PickAssignmentId,
            pickupSegmentId: fitItem.PickupSegmentId,
            previewVersion: fitItem.PreviewEvidence?.PreviewVersion,
            pickMapRevision: fitPlan.PickMapRevision,
            pickAssignmentsFingerprint: fitPlan.PickAssignmentsFingerprint);

        return new StagedReplacement(
            Id: fitItem.ReplacementId,
            ChapterStem: fitPlan.ChapterStem,
            SentenceId: fitItem.Target.SentenceId,
            OriginalStartSec: fitItem.OuterRange.StartSec,
            OriginalEndSec: fitItem.OuterRange.EndSec,
            PickupSourcePath: fitPlan.Source.Path,
            PickupStartSec: fitItem.InnerRange.StartSec,
            PickupEndSec: fitItem.InnerRange.EndSec,
            CrossfadeDurationSec: render.EffectiveCrossfadeDurationSec,
            CrossfadeCurve: render.CrossfadeCurve,
            StagedAtUtc: DateTime.UtcNow,
            Status: ReplacementStatus.Staged,
            ExplicitReplacementDurationSec: render.RenderedDurationSec,
            FitMetadata: metadata);
    }

    private void EnsureFitReplacementQueued(string chapterStem, StagedReplacement replacement)
    {
        var existing = TryFindStagedItem(replacement.Id, chapterStem);
        if (existing is null)
        {
            if (!_stagingQueue.TryStage(replacement, out var validationError))
            {
                throw new InvalidOperationException(
                    $"Pickup Fit commit failed to stage replacement for chapter '{chapterStem}', op '{replacement.Id}': {validationError}");
            }

            return;
        }

        if (existing.Status != ReplacementStatus.Staged)
        {
            throw new InvalidOperationException(
                $"Pickup Fit commit rejected for chapter '{chapterStem}', op '{replacement.Id}': queue state is '{existing.Status}', expected 'Staged'.");
        }

        if (!StagedReplacementPayloadMatches(existing, replacement))
        {
            throw new InvalidOperationException(
                $"Pickup Fit commit rejected for chapter '{chapterStem}', op '{replacement.Id}': existing staged payload differs from accepted Fit payload.");
        }
    }

    private static bool StagedReplacementPayloadMatches(StagedReplacement left, StagedReplacement right)
    {
        const double epsilon = 0.000_001;
        return string.Equals(left.ChapterStem, right.ChapterStem, StringComparison.OrdinalIgnoreCase)
               && left.SentenceId == right.SentenceId
               && string.Equals(Path.GetFullPath(left.PickupSourcePath), Path.GetFullPath(right.PickupSourcePath), StringComparison.OrdinalIgnoreCase)
               && Math.Abs(left.OriginalStartSec - right.OriginalStartSec) <= epsilon
               && Math.Abs(left.OriginalEndSec - right.OriginalEndSec) <= epsilon
               && Math.Abs(left.PickupStartSec - right.PickupStartSec) <= epsilon
               && Math.Abs(left.PickupEndSec - right.PickupEndSec) <= epsilon
               && Math.Abs(left.CrossfadeDurationSec - right.CrossfadeDurationSec) <= epsilon
               && string.Equals(left.CrossfadeCurve, right.CrossfadeCurve, StringComparison.Ordinal)
               && NullableDurationMatches(left.ExplicitReplacementDurationSec, right.ExplicitReplacementDurationSec, epsilon)
               && EqualityComparer<PickupEdlFitMetadata?>.Default.Equals(left.FitMetadata, right.FitMetadata);
    }

    private static bool NullableDurationMatches(double? left, double? right, double epsilon)
        => left is null && right is null ||
           left is not null && right is not null && Math.Abs(left.Value - right.Value) <= epsilon;

    private PickupEdlSourceReference ResolveSourceReferenceForReplacement(string chapterStem, StagedReplacement replacement)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(replacement);

        var document = _pickupEdlStore.TryRead(chapterStem, CancellationToken.None);
        if (document is not null)
        {
            var requestedPath = Path.GetFullPath(replacement.PickupSourcePath.Trim());
            if (!string.Equals(document.Source.Path, requestedPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pickup source path mismatch for chapter '{chapterStem}', op '{replacement.Id}': " +
                    $"document='{document.Source.Path}', requested='{requestedPath}', " +
                    $"fingerprint='{document.Source.Fingerprint}'.");
            }

            // Single-source chapter contract: once a document exists, source affinity is document-owned.
            return document.Source;
        }

        return _pickupSourceBufferCache.DescribeSource(replacement.PickupSourcePath);
    }

    private PickupEdlDocument UpsertPickupEdlOperation(
        string chapterStem,
        StagedReplacement replacement,
        PickupEdlOperationState state,
        IReadOnlySet<int>? knownSentenceIds,
        CancellationToken ct,
        bool validateSourceRange = true)
    {
        var source = ResolveSourceReferenceForReplacement(chapterStem, replacement);

        if (validateSourceRange)
        {
            _ = _pickupSourceBufferCache.GetSliceByTime(
                source,
                replacement.PickupStartSec,
                replacement.PickupEndSec,
                chapterStem,
                replacement.Id,
                ct);
        }

        var operation = _pickupEdlEngine.BuildOperation(
            replacement,
            source,
            state,
            knownSentenceIds,
            DateTime.UtcNow);

        var updated = _pickupEdlStore.Mutate(
            chapterStem,
            source,
            document => _pickupEdlEngine.UpsertOperation(document, operation),
            ct);

        LogPickupEdlTransition(updated, operation.Id, state, reason: "upsert");
        return updated;
    }

    private (PickupEdlDocument Document, PickupEdlSourceReference Source) EnsurePickupEdlOperationSeeded(
        string chapterStem,
        StagedReplacement replacement,
        IReadOnlySet<int>? knownSentenceIds,
        CancellationToken ct)
    {
        var current = _pickupEdlStore.TryRead(chapterStem, ct);
        if (current is not null)
        {
            var existing = _pickupEdlEngine.TryGetOperation(current, replacement.Id);
            if (existing is not null)
            {
                if (!OperationMatchesReplacement(existing, replacement))
                {
                    throw new InvalidOperationException(
                        $"Pickup commit rejected for chapter '{chapterStem}', op '{replacement.Id}': " +
                        "existing pickup EDL operation payload differs from staged queue payload.");
                }

                if (existing.State == PickupEdlOperationState.Staged)
                {
                    return (current, current.Source);
                }

                if (existing.State == PickupEdlOperationState.Failed)
                {
                    var restaged = TransitionPickupEdlOperationState(
                        chapterStem,
                        current.Source,
                        replacement.Id,
                        PickupEdlOperationState.Staged,
                        ct);

                    if (!_stagingQueue.UpdateStatus(replacement.Id, ReplacementStatus.Staged, syncEditList: false))
                    {
                        throw new InvalidOperationException(
                            $"Pickup commit failed to restage queue status for chapter '{chapterStem}', op '{replacement.Id}'.");
                    }

                    SyncPickupEditsFromEdl(chapterStem, restaged);
                    return (restaged, restaged.Source);
                }

                throw new InvalidOperationException(
                    $"Pickup commit rejected for chapter '{chapterStem}', op '{replacement.Id}': " +
                    $"existing pickup EDL state is '{existing.State}', expected 'Staged'.");
            }
        }

        var seeded = UpsertPickupEdlOperation(
            chapterStem,
            replacement,
            PickupEdlOperationState.Staged,
            knownSentenceIds,
            ct,
            validateSourceRange: true);

        return (seeded, seeded.Source);
    }

    private PickupEdlDocument TransitionPickupEdlOperationState(
        string chapterStem,
        PickupEdlSourceReference source,
        string replacementId,
        PickupEdlOperationState nextState,
        CancellationToken ct)
    {
        var updated = _pickupEdlStore.Mutate(
            chapterStem,
            source,
            document => _pickupEdlEngine.TransitionOperationState(document, replacementId, nextState, DateTime.UtcNow),
            ct);

        LogPickupEdlTransition(updated, replacementId, nextState, reason: "transition");
        return updated;
    }

    private void PersistFailedPickupState(
        string chapterStem,
        StagedReplacement replacement,
        IReadOnlySet<int>? knownSentenceIds,
        Exception error,
        CancellationToken ct)
    {
        try
        {
            var current = _pickupEdlStore.TryRead(chapterStem, ct);
            PickupEdlDocument failedDocument;

            if (current is not null && _pickupEdlEngine.TryGetOperation(current, replacement.Id) is not null)
            {
                failedDocument = TransitionPickupEdlOperationState(
                    chapterStem,
                    current.Source,
                    replacement.Id,
                    PickupEdlOperationState.Failed,
                    CancellationToken.None);
            }
            else
            {
                failedDocument = UpsertPickupEdlOperation(
                    chapterStem,
                    replacement,
                    PickupEdlOperationState.Failed,
                    knownSentenceIds,
                    CancellationToken.None,
                    validateSourceRange: false);
            }

            _stagingQueue.UpdateStatus(replacement.Id, ReplacementStatus.Failed, syncEditList: false);
            SyncPickupEditsFromEdl(chapterStem, failedDocument);

            Log.Warn(
                "Pickup operation failed: chapter={ChapterStem}, op={OperationId}, state=failed, message={Message}",
                chapterStem,
                replacement.Id,
                error.Message);
        }
        catch (Exception persistEx)
        {
            _stagingQueue.UpdateStatus(replacement.Id, ReplacementStatus.Failed, syncEditList: false);
            Log.Warn(
                "Failed to persist pickup failure state for chapter={ChapterStem}, op={OperationId}: {Message}",
                chapterStem,
                replacement.Id,
                persistEx.Message);
        }
    }

    private void SyncPickupEditsFromEdl(string chapterStem, PickupEdlDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(document);

        var existing = _editListService.GetEdits(chapterStem);
        var nonPickupEdits = existing
            .Where(edit => edit.Operation != EditOperation.PickupReplace)
            .ToArray();

        var pickupEdits = _pickupEdlEngine.BuildAppliedProjectionEdits(document);
        var merged = nonPickupEdits
            .Concat(pickupEdits)
            .OrderBy(edit => edit.BaselineStartSec)
            .ThenBy(edit => edit.BaselineEndSec)
            .ThenBy(edit => edit.Id, StringComparer.Ordinal)
            .ToArray();

        _editListService.ReplaceChapterEdits(chapterStem, merged);

        var lastValidationError = document.Operations
            .Where(op => op.State == PickupEdlOperationState.Failed)
            .OrderByDescending(op => op.UpdatedAtUtc)
            .Select(op => op.Id)
            .FirstOrDefault();

        Log.Info(
            "Pickup EDL sync: chapter={ChapterStem}, revision={Revision}, appliedOps={AppliedOps}, totalEdits={TotalEdits}, diagnostics={Diagnostics}, lastValidationError={LastValidationError}",
            document.ChapterStem,
            document.Revision,
            pickupEdits.Count,
            merged.Length,
            _pickupEdlEngine.BuildDeterministicOrderingDiagnostics(document),
            lastValidationError ?? "<none>");
    }

    private static bool OperationMatchesReplacement(PickupEdlOperation operation, StagedReplacement replacement)
    {
        const double epsilon = 0.000_001;

        return Math.Abs(operation.BaselineStartSec - replacement.OriginalStartSec) <= epsilon
               && Math.Abs(operation.BaselineEndSec - replacement.OriginalEndSec) <= epsilon
               && Math.Abs(operation.SourceStartSec - replacement.PickupStartSec) <= epsilon
               && Math.Abs(operation.SourceEndSec - replacement.PickupEndSec) <= epsilon
               && Math.Abs(operation.CrossfadeDurationSec - replacement.CrossfadeDurationSec) <= epsilon
               && string.Equals(operation.CrossfadeCurve, replacement.CrossfadeCurve, StringComparison.Ordinal)
               && operation.SentenceId == replacement.SentenceId
               && NullableDurationMatches(operation.ExplicitReplacementDurationSec, replacement.ExplicitReplacementDurationSec, epsilon)
               && EqualityComparer<PickupEdlFitMetadata?>.Default.Equals(operation.FitMetadata, replacement.FitMetadata);
    }

    private void LogPickupEdlTransition(
        PickupEdlDocument document,
        string operationId,
        PickupEdlOperationState state,
        string reason)
    {
        Log.Info(
            "Pickup EDL transition: chapter={ChapterStem}, revision={Revision}, op={OperationId}, state={State}, reason={Reason}, diagnostics={Diagnostics}",
            document.ChapterStem,
            document.Revision,
            operationId,
            state,
            reason,
            _pickupEdlEngine.BuildDeterministicOrderingDiagnostics(document));
    }

    private bool TryAppendPickupArtifactLedgerEntry(
        string chapterStem,
        string operationId,
        string transition,
        string phase,
        int edlRevision,
        ReplacementStatus queueStatus,
        PickupEdlOperationState edlState,
        PickupArtifactLedgerRollbackVerdict rollbackVerdict,
        string? failureReason,
        CancellationToken ct)
    {
        try
        {
            var artifactRefs = BuildPickupArtifactRefs(chapterStem);
            var updated = _pickupArtifactLedgerStore.Append(
                chapterStem,
                new PickupArtifactLedgerEntryDraft(
                    operationId: operationId,
                    transition: transition,
                    phase: phase,
                    edlRevision: edlRevision,
                    queueStatus: queueStatus,
                    edlState: edlState,
                    rollbackVerdict: rollbackVerdict,
                    artifactRefs: artifactRefs,
                    failureReason: failureReason,
                    occurredAtUtc: DateTime.UtcNow),
                ct);

            Log.Info(
                "Pickup artifact ledger transition: chapter={ChapterStem}, revision={Revision}, op={OperationId}, transition={Transition}, phase={Phase}, rollback={RollbackVerdict}",
                chapterStem,
                updated.Revision,
                operationId,
                transition,
                phase,
                rollbackVerdict);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warn(
                "Pickup artifact ledger append failed: chapter={ChapterStem}, op={OperationId}, transition={Transition}, phase={Phase}, message={Message}",
                chapterStem,
                operationId,
                transition,
                phase,
                ex.Message);
            return false;
        }
    }

    private static IReadOnlyList<string> BuildPickupArtifactRefs(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        return
        [
            $".polish/edl/{chapterStem}.artifact-ledger.json",
            $".polish/edl/{chapterStem}.edl.json"
        ];
    }

    private int ResolveCurrentPickupEdlRevision(string chapterStem, CancellationToken ct)
    {
        try
        {
            var current = _pickupEdlStore.TryRead(chapterStem, ct);
            return current?.Revision ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private ReplacementStatus ResolveQueueStatus(string chapterStem, string replacementId, ReplacementStatus fallback)
    {
        var item = TryFindStagedItem(replacementId, chapterStem);
        return item?.Status ?? fallback;
    }

    private static void EnsureReplacementStatusForApply(StagedReplacement replacement, string chapterStem)
    {
        if (replacement.Status != ReplacementStatus.Staged)
        {
            throw new InvalidOperationException(
                $"Pickup commit rejected for chapter '{chapterStem}', op '{replacement.Id}': " +
                $"queue state is '{replacement.Status}', expected 'Staged'.");
        }
    }

    private static void EnsureReplacementStatusForRevert(StagedReplacement replacement, string chapterStem)
    {
        if (replacement.Status != ReplacementStatus.Applied)
        {
            throw new InvalidOperationException(
                $"Pickup revert rejected for chapter '{chapterStem}', op '{replacement.Id}': " +
                $"queue state is '{replacement.Status}', expected 'Applied'.");
        }
    }

    internal bool TryRollbackPickupTransition(
        string chapterStem,
        PickupEdlSourceReference source,
        string replacementId,
        PickupEdlOperationState rollbackEdlState,
        ReplacementStatus rollbackQueueStatus,
        string phase,
        Exception trigger)
    {
        try
        {
            var rolledBack = TransitionPickupEdlOperationState(
                chapterStem,
                source,
                replacementId,
                rollbackEdlState,
                CancellationToken.None);

            if (!_stagingQueue.UpdateStatus(replacementId, rollbackQueueStatus, syncEditList: false))
            {
                throw new InvalidOperationException(
                    $"Pickup rollback failed to update queue status for chapter '{chapterStem}', op '{replacementId}'.");
            }

            SyncPickupEditsFromEdl(chapterStem, rolledBack);

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: chapterStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.RollbackSucceeded,
                phase: phase,
                edlRevision: rolledBack.Revision,
                queueStatus: rollbackQueueStatus,
                edlState: rollbackEdlState,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.Succeeded,
                failureReason: trigger.Message,
                CancellationToken.None);

            Log.Warn(
                "Pickup lifecycle rollback succeeded: chapter={ChapterStem}, op={OperationId}, phase={Phase}, rollbackState={RollbackState}, revision={Revision}, trigger={Trigger}",
                chapterStem,
                replacementId,
                phase,
                rollbackEdlState,
                rolledBack.Revision,
                trigger.Message);
            return true;
        }
        catch (Exception rollbackEx)
        {
            TryAppendPickupArtifactLedgerEntry(
                chapterStem: chapterStem,
                operationId: replacementId,
                transition: PickupArtifactLedgerTransitions.RollbackFailed,
                phase: phase,
                edlRevision: ResolveCurrentPickupEdlRevision(chapterStem, CancellationToken.None),
                queueStatus: ResolveQueueStatus(chapterStem, replacementId, ReplacementStatus.Failed),
                edlState: PickupEdlOperationState.Failed,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.Failed,
                failureReason: rollbackEx.Message,
                CancellationToken.None);

            Log.Warn(
                "Pickup lifecycle rollback failed: chapter={ChapterStem}, op={OperationId}, phase={Phase}, rollbackState={RollbackState}, trigger={Trigger}, message={Message}",
                chapterStem,
                replacementId,
                phase,
                rollbackEdlState,
                trigger.Message,
                rollbackEx.Message);
            return false;
        }
    }

    internal bool TryRollbackBatchPickupTransitions(
        string chapterStem,
        IReadOnlyList<(StagedReplacement Item, PickupEdlSourceReference Source)> transitioned,
        PickupEdlOperationState rollbackEdlState,
        ReplacementStatus rollbackQueueStatus,
        string phase,
        Exception trigger)
    {
        if (transitioned.Count == 0)
        {
            return true;
        }

        try
        {
            PickupEdlDocument? latest = null;

            for (var i = transitioned.Count - 1; i >= 0; i--)
            {
                var (item, source) = transitioned[i];

                latest = TransitionPickupEdlOperationState(
                    chapterStem,
                    source,
                    item.Id,
                    rollbackEdlState,
                    CancellationToken.None);

                if (!_stagingQueue.UpdateStatus(item.Id, rollbackQueueStatus, syncEditList: false))
                {
                    throw new InvalidOperationException(
                        $"Batch pickup rollback failed to update queue status for chapter '{chapterStem}', op '{item.Id}'.");
                }

                TryAppendPickupArtifactLedgerEntry(
                    chapterStem: chapterStem,
                    operationId: item.Id,
                    transition: PickupArtifactLedgerTransitions.RollbackSucceeded,
                    phase: phase,
                    edlRevision: latest.Revision,
                    queueStatus: rollbackQueueStatus,
                    edlState: rollbackEdlState,
                    rollbackVerdict: PickupArtifactLedgerRollbackVerdict.Succeeded,
                    failureReason: trigger.Message,
                    CancellationToken.None);
            }

            if (latest is not null)
            {
                SyncPickupEditsFromEdl(chapterStem, latest);
            }

            Log.Warn(
                "Batch pickup lifecycle rollback succeeded: chapter={ChapterStem}, phase={Phase}, rollbackState={RollbackState}, opCount={OperationCount}, revision={Revision}, trigger={Trigger}",
                chapterStem,
                phase,
                rollbackEdlState,
                transitioned.Count,
                latest?.Revision ?? -1,
                trigger.Message);
            return true;
        }
        catch (Exception rollbackEx)
        {
            foreach (var (item, _) in transitioned)
            {
                TryAppendPickupArtifactLedgerEntry(
                    chapterStem: chapterStem,
                    operationId: item.Id,
                    transition: PickupArtifactLedgerTransitions.RollbackFailed,
                    phase: phase,
                    edlRevision: ResolveCurrentPickupEdlRevision(chapterStem, CancellationToken.None),
                    queueStatus: ResolveQueueStatus(chapterStem, item.Id, ReplacementStatus.Failed),
                    edlState: PickupEdlOperationState.Failed,
                    rollbackVerdict: PickupArtifactLedgerRollbackVerdict.Failed,
                    failureReason: rollbackEx.Message,
                    CancellationToken.None);
            }

            Log.Warn(
                "Batch pickup lifecycle rollback failed: chapter={ChapterStem}, phase={Phase}, rollbackState={RollbackState}, opCount={OperationCount}, trigger={Trigger}, message={Message}",
                chapterStem,
                phase,
                rollbackEdlState,
                transitioned.Count,
                trigger.Message,
                rollbackEx.Message);
            return false;
        }
    }

    internal bool TryRestorePickupOperationSnapshot(
        string chapterStem,
        PickupEdlSourceReference source,
        PickupEdlOperation operationSnapshot,
        ReplacementStatus rollbackQueueStatus,
        string phase,
        Exception trigger)
    {
        try
        {
            var restoredOperation = operationSnapshot;
            var restored = _pickupEdlStore.Mutate(
                chapterStem,
                source,
                document => _pickupEdlEngine.UpsertOperation(document, restoredOperation),
                CancellationToken.None);

            LogPickupEdlTransition(restored, restoredOperation.Id, restoredOperation.State, reason: $"{phase}-snapshot-restore");

            if (!_stagingQueue.UpdateStatus(restoredOperation.Id, rollbackQueueStatus, syncEditList: false))
            {
                throw new InvalidOperationException(
                    $"Pickup snapshot rollback failed to update queue status for chapter '{chapterStem}', op '{restoredOperation.Id}'.");
            }

            SyncPickupEditsFromEdl(chapterStem, restored);

            TryAppendPickupArtifactLedgerEntry(
                chapterStem: chapterStem,
                operationId: restoredOperation.Id,
                transition: PickupArtifactLedgerTransitions.RollbackSucceeded,
                phase: phase,
                edlRevision: restored.Revision,
                queueStatus: rollbackQueueStatus,
                edlState: restoredOperation.State,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.Succeeded,
                failureReason: trigger.Message,
                CancellationToken.None);

            Log.Warn(
                "Pickup snapshot rollback succeeded: chapter={ChapterStem}, op={OperationId}, phase={Phase}, rollbackState={RollbackState}, revision={Revision}, trigger={Trigger}",
                chapterStem,
                restoredOperation.Id,
                phase,
                restoredOperation.State,
                restored.Revision,
                trigger.Message);
            return true;
        }
        catch (Exception rollbackEx)
        {
            TryAppendPickupArtifactLedgerEntry(
                chapterStem: chapterStem,
                operationId: operationSnapshot.Id,
                transition: PickupArtifactLedgerTransitions.RollbackFailed,
                phase: phase,
                edlRevision: ResolveCurrentPickupEdlRevision(chapterStem, CancellationToken.None),
                queueStatus: ResolveQueueStatus(chapterStem, operationSnapshot.Id, ReplacementStatus.Failed),
                edlState: PickupEdlOperationState.Failed,
                rollbackVerdict: PickupArtifactLedgerRollbackVerdict.Failed,
                failureReason: rollbackEx.Message,
                CancellationToken.None);

            Log.Warn(
                "Pickup snapshot rollback failed: chapter={ChapterStem}, op={OperationId}, phase={Phase}, rollbackState={RollbackState}, trigger={Trigger}, message={Message}",
                chapterStem,
                operationSnapshot.Id,
                phase,
                operationSnapshot.State,
                trigger.Message,
                rollbackEx.Message);
            return false;
        }
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
    /// Uses the pickup source cache so one decoded buffer can serve many slice views.
    /// </summary>
    private AudioBuffer LoadPickupSliceForReplacement(
        string chapterStem,
        StagedReplacement item,
        PickupEdlSourceReference source,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(source);

        var pickupBuffer = _pickupSourceBufferCache.GetSourceBuffer(source, chapterStem, item.Id, ct);
        var pickupDurationSec = (double)pickupBuffer.Length / pickupBuffer.SampleRate;

        // Content-aware handle: crossfade + guard ensures crossfade fits outside speech.
        var handlePaddingSec = item.CrossfadeDurationSec + HandleGuardSec;
        var paddedStartSec = Math.Max(0, item.PickupStartSec - handlePaddingSec);
        var paddedEndSec = Math.Min(pickupDurationSec, item.PickupEndSec + handlePaddingSec);

        if (paddedEndSec <= paddedStartSec)
        {
            paddedStartSec = Math.Max(0, item.PickupStartSec);
            paddedEndSec = Math.Min(pickupDurationSec, item.PickupEndSec);
            if (paddedEndSec <= paddedStartSec)
            {
                paddedEndSec = Math.Min(pickupDurationSec, paddedStartSec + 0.010);
            }
        }

        return _pickupSourceBufferCache.GetSliceByTime(
            source,
            paddedStartSec,
            paddedEndSec,
            chapterStem,
            item.Id,
            ct);
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
