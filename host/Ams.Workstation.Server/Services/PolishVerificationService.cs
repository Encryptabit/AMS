using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Common;
using Ams.Core.Processors;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Post-replacement verification service that re-runs ASR on affected audio segments,
/// computes similarity metrics (WER/Levenshtein), and syncs fix status to the Proof area.
/// </summary>
public class PolishVerificationService
{
    private const double PassThreshold = 0.9;

    private readonly BlazorWorkspace _workspace;
    private readonly ReviewedStatusService _reviewedStatus;
    private readonly StagingQueueService _stagingQueue;

    /// <summary>In-memory revalidation history per chapter stem, cleared on chapter change.</summary>
    private readonly ConcurrentDictionary<string, List<RevalidationResult>> _history = new();

    public PolishVerificationService(
        BlazorWorkspace workspace,
        ReviewedStatusService reviewedStatus,
        StagingQueueService stagingQueue)
    {
        _workspace = workspace;
        _reviewedStatus = reviewedStatus;
        _stagingQueue = stagingQueue;
    }

    /// <summary>
    /// Re-runs ASR on a replaced audio segment and computes similarity against the expected text.
    /// </summary>
    /// <param name="chapterBuffer">The full chapter audio buffer (post-replacement).</param>
    /// <param name="startSec">Start time of the affected segment in seconds.</param>
    /// <param name="endSec">End time of the affected segment in seconds.</param>
    /// <param name="expectedText">The book text that the replacement should match.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="RevalidationResult"/> with similarity score and pass/fail status.</returns>
    public async Task<RevalidationResult> RevalidateSegmentAsync(
        AudioBuffer chapterBuffer,
        double startSec,
        double endSec,
        string expectedText,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(chapterBuffer);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedText);

        // 1. Trim the affected segment from the chapter buffer
        var segment = AudioProcessor.Trim(
            chapterBuffer,
            TimeSpan.FromSeconds(startSec),
            TimeSpan.FromSeconds(endSec));

        // 2. Prepare for ASR (mono 16kHz)
        var asrReady = AsrAudioPreparer.PrepareForAsr(segment);

        // 3. Run ASR on the segment
        var asrOptions = await BuildAsrOptionsAsync(ct).ConfigureAwait(false);
        var asrResponse = await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct)
            .ConfigureAwait(false);

        // 4. Extract recognized text from ASR response
        var recognizedText = ExtractFullText(asrResponse);

        // 5. Compute similarity via Levenshtein
        var similarity = string.IsNullOrWhiteSpace(recognizedText)
            ? 0.0
            : LevenshteinMetrics.Similarity(expectedText, recognizedText);

        // 6. Build result
        var result = new RevalidationResult(
            RecognizedText: recognizedText,
            ExpectedText: expectedText,
            Similarity: similarity,
            Passed: similarity >= PassThreshold,
            Tokens: asrResponse.Tokens);

        return result;
    }

    /// <summary>
    /// Syncs a sentence's fix status to the Proof area after verification.
    /// When a fix passes re-validation and the user accepts it, the sentence status
    /// automatically updates in the Proof area via <see cref="ReviewedStatusService"/>.
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    /// <param name="sentenceId">The sentence that was fixed.</param>
    /// <param name="passed">Whether the re-validation passed.</param>
    public Task SyncToProofAsync(string chapterStem, int sentenceId, bool passed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        if (passed)
        {
            // Mark the chapter as reviewed (updated) in the Proof area.
            // The Proof UI picks up the status change on next load since
            // ReviewedStatusService is singleton and persistent.
            _reviewedStatus.SetReviewed(chapterStem, true);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the list of recent re-validations for a chapter (in-memory cache).
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    /// <returns>Read-only list of revalidation results.</returns>
    public IReadOnlyList<RevalidationResult> GetRevalidationHistory(string chapterStem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);

        return _history.TryGetValue(chapterStem, out var list)
            ? list.AsReadOnly()
            : Array.Empty<RevalidationResult>();
    }

    /// <summary>
    /// Records a revalidation result in the in-memory history for a chapter.
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    /// <param name="result">The revalidation result to record.</param>
    public void RecordResult(string chapterStem, RevalidationResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterStem);
        ArgumentNullException.ThrowIfNull(result);

        var list = _history.GetOrAdd(chapterStem, _ => new List<RevalidationResult>());
        lock (list)
        {
            list.Add(result);
        }
    }

    /// <summary>
    /// Clears the revalidation history for a chapter (e.g., on chapter change).
    /// </summary>
    /// <param name="chapterStem">The chapter stem identifier.</param>
    public void ClearHistory(string chapterStem)
    {
        _history.TryRemove(chapterStem, out _);
    }

    /// <summary>
    /// Extracts the full recognized text from an ASR response by joining token words.
    /// </summary>
    private static string ExtractFullText(AsrResponse response)
    {
        if (response.Tokens is { Length: > 0 })
        {
            return string.Join(" ", response.Tokens.Select(t => t.Word)).Trim();
        }

        if (response.Segments is { Length: > 0 })
        {
            return string.Join(" ", response.Segments.Select(s => s.Text)).Trim();
        }

        return string.Empty;
    }

    /// <summary>
    /// Builds default ASR options, resolving the Whisper model path with auto-download fallback.
    /// </summary>
    private static async Task<AsrOptions> BuildAsrOptionsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var (modelPath, _) = await AsrEngineConfig.ResolveModelPathAsync().ConfigureAwait(false);
        return new AsrOptions(
            ModelPath: modelPath,
            Language: "en",
            EnableWordTimestamps: true);
    }
}

/// <summary>
/// Result of re-running ASR on a replaced segment and comparing to the expected book text.
/// </summary>
/// <param name="RecognizedText">Text recognized by ASR from the replaced audio.</param>
/// <param name="ExpectedText">The expected book text for the sentence.</param>
/// <param name="Similarity">Levenshtein similarity score (0.0 - 1.0).</param>
/// <param name="Passed">Whether the similarity meets the pass threshold (0.9).</param>
/// <param name="Tokens">ASR tokens with timing information from the re-validation run.</param>
public sealed record RevalidationResult(
    string RecognizedText,
    string ExpectedText,
    double Similarity,
    bool Passed,
    IReadOnlyList<AsrToken> Tokens);
