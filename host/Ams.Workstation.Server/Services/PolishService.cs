using System;
using System.Collections.Generic;
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

    public PolishService(
        BlazorWorkspace workspace,
        StagingQueueService stagingQueue,
        UndoService undoService,
        PickupMatchingService pickupMatching)
    {
        _workspace = workspace;
        _stagingQueue = stagingQueue;
        _undoService = undoService;
        _pickupMatching = pickupMatching;
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

        var replacement = new StagedReplacement(
            Id: Guid.NewGuid().ToString("N"),
            ChapterStem: chapterStem,
            SentenceId: match.SentenceId,
            OriginalStartSec: originalStartSec,
            OriginalEndSec: originalEndSec,
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
    /// Applies a staged replacement: backs up the original segment, splices in
    /// the pickup audio with crossfade, and records the timing delta.
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

        // 2. Load chapter audio from workspace
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

        // 7. Update status to Applied
        _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Applied);

        return (resultBuffer, timingDelta);
    }

    /// <summary>
    /// Reverts a previously applied replacement by restoring the original audio segment
    /// from the undo backup.
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

        // 4. Calculate where the replacement currently sits (accounting for previous shifts)
        // The replacement was applied at the original boundaries, and its duration may differ.
        // For revert, we replace the current replacement region with the backed-up original.
        var replacementDuration = undoRecord.ReplacementDurationSec;
        var replacementEndSec = undoRecord.OriginalStartSec + replacementDuration;

        // 5. Re-splice: put the original back
        var resultBuffer = AudioSpliceService.ReplaceSegment(
            currentBuffer,
            undoRecord.OriginalStartSec,
            replacementEndSec,
            originalSegment,
            0.030,
            "tri");

        // 6. Timing delta is negative of original delta
        var timingDelta = undoRecord.OriginalDurationSec - replacementDuration;

        // 7. Update status to Reverted
        _stagingQueue.UpdateStatus(replacementId, ReplacementStatus.Reverted);

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
        // Search across all chapters for the replacement
        var allQueued = _stagingQueue.GetAllQueued();
        foreach (var item in allQueued)
        {
            if (item.Id == replacementId)
                return item;
        }

        // Also check non-Staged items (Applied status items for revert scenarios)
        // Fall back to searching all chapters
        throw new InvalidOperationException($"Staged replacement '{replacementId}' not found.");
    }

    private AudioBuffer GetCurrentChapterBuffer()
    {
        var handle = _workspace.CurrentChapterHandle
            ?? throw new InvalidOperationException("No chapter is currently selected.");

        return handle.Chapter.Audio.Current.Buffer;
    }

    #endregion
}
