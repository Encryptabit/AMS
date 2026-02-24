using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Audio;
using Ams.Core.Processors;
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
    /// Imports a pickup recording by running ASR-based matching against flagged sentences.
    /// </summary>
    /// <param name="pickupFilePath">Path to the pickup WAV file.</param>
    /// <param name="flaggedSentences">CRX target sentences to match against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of pickup matches for UI display.</returns>
    public Task<List<PickupMatch>> ImportPickupAsync(
        string pickupFilePath,
        IReadOnlyList<HydratedSentence> flaggedSentences,
        CancellationToken ct)
    {
        return _pickupMatching.MatchPickupAsync(pickupFilePath, flaggedSentences, ct);
    }

    /// <summary>
    /// Creates a <see cref="StagedReplacement"/> from a match and adds it to the staging queue.
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    /// <param name="match">The pickup match to stage.</param>
    /// <param name="pickupFilePath">Path to the pickup source file.</param>
    /// <param name="originalStartSec">Start time of the original sentence in the chapter audio.</param>
    /// <param name="originalEndSec">End time of the original sentence in the chapter audio.</param>
    /// <param name="crossfadeSec">Crossfade duration in seconds (default 30ms).</param>
    /// <param name="curve">Crossfade curve type (default "tri").</param>
    /// <returns>The created StagedReplacement record.</returns>
    public StagedReplacement StageReplacement(
        string chapterStem,
        PickupMatch match,
        string pickupFilePath,
        double originalStartSec,
        double originalEndSec,
        double crossfadeSec = 0.030,
        string curve = "tri")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(match);
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);

        // Refine splice boundaries using silence detection
        var refinedStart = originalStartSec;
        var refinedEnd = originalEndSec;
        try
        {
            var chapterBuffer = GetCurrentChapterBuffer();
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
                    prevEnd = sentences[idx - 1].Timing?.EndSec;
                if (idx >= 0 && idx < sentences.Count - 1)
                    nextStart = sentences[idx + 1].Timing?.StartSec;
            }

            var result = SpliceBoundaryService.RefineBoundaries(
                chapterBuffer, originalStartSec, originalEndSec,
                prevEnd, nextStart);

            refinedStart = result.RefinedStartSec;
            refinedEnd = result.RefinedEndSec;

            Console.WriteLine(
                $"[BoundaryRefinement] Sentence {match.SentenceId}: " +
                $"start {originalStartSec:F3}s → {refinedStart:F3}s ({result.StartMethod}), " +
                $"end {originalEndSec:F3}s → {refinedEnd:F3}s ({result.EndMethod})");
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
            CrossfadeDurationSec: crossfadeSec,
            CrossfadeCurve: curve,
            StagedAtUtc: DateTime.UtcNow,
            Status: ReplacementStatus.Staged);

        _stagingQueue.Stage(replacement);
        return replacement;
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

        var item = FindStagedItem(replacementId);
        var chapterBuffer = GetCurrentChapterBuffer();

        var pickupBuffer = AudioProcessor.Decode(item.PickupSourcePath);
        var pickupTrimmed = AudioProcessor.Trim(
            pickupBuffer,
            TimeSpan.FromSeconds(item.PickupStartSec),
            TimeSpan.FromSeconds(item.PickupEndSec));

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

        // 1. Get staged item
        var item = FindStagedItem(replacementId);

        // 2. Load chapter audio from workspace (corrected > treated > raw)
        var chapterBuffer = GetCurrentChapterBuffer();

        // 3. Decode and trim pickup audio
        var pickupBuffer = AudioProcessor.Decode(item.PickupSourcePath);
        var pickupTrimmed = AudioProcessor.Trim(
            pickupBuffer,
            TimeSpan.FromSeconds(item.PickupStartSec),
            TimeSpan.FromSeconds(item.PickupEndSec));

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
        PersistCorrectedBuffer(resultBuffer);

        // 8. Update status to Applied
        _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Applied);

        // 9. Cascade timing delta to downstream staged items
        _stagingQueue.ShiftDownstream(item.ChapterStem, item.SentenceId, timingDelta);

        return (resultBuffer, timingDelta);
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

        // 1. Get undo record
        var undoRecord = _undoService.GetUndoRecord(replacementId)
            ?? throw new InvalidOperationException($"No undo record found for replacement '{replacementId}'.");

        // 2. Load original segment from backup
        var originalSegment = _undoService.LoadOriginalSegment(replacementId)
            ?? throw new InvalidOperationException($"Undo backup file missing for replacement '{replacementId}'.");

        // 3. Get current chapter audio (which has the replacement applied)
        var currentBuffer = GetCurrentChapterBuffer();

        // 4. Calculate where the replacement currently sits.
        // Use the queue item's current (shifted) coordinates rather than the stale undo record,
        // because upstream apply/revert may have cascaded timing deltas since this item was applied.
        var queueItem = FindStagedItem(replacementId);
        var currentStartSec = queueItem.OriginalStartSec;
        var replacementDuration = undoRecord.ReplacementDurationSec;
        var replacementEndSec = currentStartSec + replacementDuration;

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
        PersistCorrectedBuffer(resultBuffer);

        // 8. Update status to Reverted
        _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Reverted);

        // 9. Cascade the negative delta to downstream staged items
        _stagingQueue.ShiftDownstream(undoRecord.ChapterStem, undoRecord.SentenceId, timingDelta);

        return (resultBuffer, timingDelta);
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

    #region Private Helpers

    private StagedReplacement FindStagedItem(string replacementId)
    {
        // Resolve the current chapter stem so we can search its full queue
        // (GetAllQueued only returns Staged items; we need Applied items too for preview/revert)
        var handle = _workspace.CurrentChapterHandle;
        if (handle is not null)
        {
            var stem = handle.Chapter.Descriptor.ChapterId;
            var chapterQueue = _stagingQueue.GetQueue(stem);
            foreach (var item in chapterQueue)
            {
                if (item.Id == replacementId)
                    return item;
            }
        }

        // Fall back to Staged-only cross-chapter search
        var allQueued = _stagingQueue.GetAllQueued();
        foreach (var item in allQueued)
        {
            if (item.Id == replacementId)
                return item;
        }

        throw new InvalidOperationException($"Replacement '{replacementId}' not found.");
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
    /// Resolves the best available chapter audio buffer: corrected.wav > treated.wav > raw.
    /// Uses direct AudioProcessor.Decode to avoid moving AudioBufferManager cursor.
    /// </summary>
    private AudioBuffer GetCurrentChapterBuffer()
    {
        var handle = _workspace.CurrentChapterHandle
            ?? throw new InvalidOperationException("No chapter is currently selected.");

        var chapter = handle.Chapter;
        var descriptor = chapter.Descriptor;
        var correctedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.corrected.wav");

        if (File.Exists(correctedPath))
            return AudioProcessor.Decode(correctedPath);

        var treatedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.treated.wav");
        if (File.Exists(treatedPath))
            return AudioProcessor.Decode(treatedPath);

        // Fall back to raw buffer via AudioBufferManager
        return chapter.Audio.Current.Buffer
            ?? throw new InvalidOperationException("No audio buffer available for the current chapter.");
    }

    /// <summary>
    /// Writes the result buffer to {stem}.corrected.wav and flushes the cached
    /// "corrected" AudioBufferContext so it reloads from disk on next access.
    /// Also clears any preview buffer.
    /// </summary>
    private void PersistCorrectedBuffer(AudioBuffer buffer)
    {
        var handle = _workspace.CurrentChapterHandle
            ?? throw new InvalidOperationException("No chapter is currently selected.");

        var descriptor = handle.Chapter.Descriptor;
        var correctedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.corrected.wav");

        AudioProcessor.EncodeWav(correctedPath, buffer);

        // Flush the cached buffer so next load picks up the new file
        handle.Chapter.Audio.Deallocate("corrected");

        // Clear any in-memory preview
        _previewBuffer.Clear();
    }

    #endregion
}
