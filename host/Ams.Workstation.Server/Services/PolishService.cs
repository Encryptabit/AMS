using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Audio;
using Ams.Core.Common;
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
    private const double PickupSlicePaddingSec = 0.080;

    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

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
    /// Imports a pickup recording by running ASR+MFA matching against ALL chapters' flagged sentences
    /// in a single pass. Returns matches distributed by chapter stem.
    /// Handles cross-chapter sentence ID collisions via text similarity disambiguation.
    /// </summary>
    /// <param name="pickupFilePath">Path to the pickup WAV file.</param>
    /// <param name="chapterTargets">Per-chapter flagged sentences to match against.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary of chapter stem to list of cross-chapter pickup matches.</returns>
    public async Task<Dictionary<string, List<CrossChapterPickupMatch>>> ImportPickupsCrossChapterAsync(
        string pickupFilePath,
        IReadOnlyList<(string ChapterStem, string ChapterName, IReadOnlyList<HydratedSentence> FlaggedSentences)> chapterTargets,
        IProgress<(string Status, double Progress)>? progress = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(chapterTargets);

        // 1. Gather ALL flagged sentences from all chapters into a single list.
        //    Track sentenceId -> list of (chapterStem, sentence) for disambiguation.
        var sentenceOwnership = new Dictionary<int, List<(string ChapterStem, HydratedSentence Sentence)>>();
        var allFlaggedSentences = new List<HydratedSentence>();

        foreach (var (chapterStem, _, flagged) in chapterTargets)
        {
            foreach (var sentence in flagged)
            {
                if (!sentenceOwnership.TryGetValue(sentence.Id, out var owners))
                {
                    owners = new List<(string, HydratedSentence)>();
                    sentenceOwnership[sentence.Id] = owners;
                }
                owners.Add((chapterStem, sentence));

                // Only add the sentence once to the matching pool if this is the first occurrence
                if (owners.Count == 1)
                    allFlaggedSentences.Add(sentence);
            }
        }

        if (allFlaggedSentences.Count == 0)
            return new Dictionary<string, List<CrossChapterPickupMatch>>();

        // 2. Report progress: splitting/ASR/MFA
        progress?.Report(("Splitting by silence...", 0.1));

        // 3. Run matching on all flagged sentences in one pass
        var matches = await _pickupMatching.MatchPickupAsync(pickupFilePath, allFlaggedSentences, ct)
            .ConfigureAwait(false);

        // 4. Distribute results into per-chapter dictionary
        progress?.Report(("Distributing matches...", 0.8));

        var result = new Dictionary<string, List<CrossChapterPickupMatch>>(StringComparer.OrdinalIgnoreCase);

        foreach (var match in matches)
        {
            ct.ThrowIfCancellationRequested();

            if (!sentenceOwnership.TryGetValue(match.SentenceId, out var owners))
                continue;

            string ownerStem;
            if (owners.Count == 1)
            {
                // No collision -- single owner
                ownerStem = owners[0].ChapterStem;
            }
            else
            {
                // Cross-chapter ID collision: disambiguate by text similarity
                ownerStem = DisambiguateOwner(match, owners);
            }

            if (!result.TryGetValue(ownerStem, out var chapterMatches))
            {
                chapterMatches = new List<CrossChapterPickupMatch>();
                result[ownerStem] = chapterMatches;
            }

            chapterMatches.Add(new CrossChapterPickupMatch(ownerStem, match));
        }

        // 5. Complete
        progress?.Report(("Complete", 1.0));

        return result;
    }

    /// <summary>
    /// Disambiguates which chapter owns a match when the same sentence ID appears in multiple chapters.
    /// Uses Levenshtein text similarity between recognized text and each chapter's sentence text.
    /// </summary>
    private static string DisambiguateOwner(
        PickupMatch match,
        List<(string ChapterStem, HydratedSentence Sentence)> owners)
    {
        var normalizedRecognized = NormalizeForCompare(match.RecognizedText);
        double bestScore = -1;
        string bestStem = owners[0].ChapterStem;

        foreach (var (stem, sentence) in owners)
        {
            var normalizedBook = NormalizeForCompare(sentence.BookText);
            var score = LevenshteinMetrics.Similarity(normalizedRecognized, normalizedBook);
            if (score > bestScore)
            {
                bestScore = score;
                bestStem = stem;
            }
        }

        return bestStem;
    }

    /// <summary>
    /// Normalizes text for comparison: lowercase, collapse whitespace, remove punctuation.
    /// </summary>
    private static string NormalizeForCompare(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.ToLowerInvariant().Trim();
        normalized = PunctuationRegex.Replace(normalized, " ");
        normalized = WhitespaceRegex.Replace(normalized, " ").Trim();
        return normalized;
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
        var pickupTrimmed = TrimPickupForReplacement(item, pickupBuffer);

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
        var pickupTrimmed = TrimPickupForReplacement(item, pickupBuffer);

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

    private static AudioBuffer TrimPickupForReplacement(StagedReplacement item, AudioBuffer pickupBuffer)
    {
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
    private void PersistCorrectedBuffer(AudioBuffer buffer)
    {
        var handle = _workspace.CurrentChapterHandle
            ?? throw new InvalidOperationException("No chapter is currently selected.");

        var descriptor = handle.Chapter.Descriptor;
        var correctedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.corrected.wav");

        // Determine source bit depth for format-preserving encode
        var treatedPath = Path.Combine(descriptor.RootPath, $"{descriptor.ChapterId}.treated.wav");
        var sourcePath = File.Exists(correctedPath) ? correctedPath : treatedPath;
        int? sourceBitDepth = null;
        if (File.Exists(sourcePath))
        {
            var info = AudioProcessor.Probe(sourcePath);
            sourceBitDepth = info.BitsPerSample > 0 ? info.BitsPerSample : null;
        }

        var options = new AudioEncodeOptions(
            TargetSampleRate: buffer.SampleRate,
            TargetBitDepth: sourceBitDepth);
        AudioProcessor.EncodeWav(correctedPath, buffer, options);

        // Flush the cached buffer so next load picks up the new file
        handle.Chapter.Audio.Deallocate("corrected");

        // Clear any in-memory preview
        _previewBuffer.Clear();
    }

    #endregion
}
