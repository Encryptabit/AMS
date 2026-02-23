using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Runs ASR on pickup recording files and matches recognized text to
/// flagged CRX target sentences using fuzzy Levenshtein similarity.
/// Supports both multi-pickup session files (segmented by silence) and
/// individual pickup files (one per sentence).
/// Uses Whisper.NET in-process via <see cref="AsrProcessor"/> exclusively.
/// </summary>
public class PickupMatchingService
{
    private const double MinSegmentDurationSec = 0.3;
    private const double MatchConfidenceThreshold = 0.5;

    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Processes a pickup recording (session or individual), runs ASR on detected
    /// speech segments, and fuzzy-matches each to the best candidate in
    /// <paramref name="flaggedSentences"/>.
    /// </summary>
    /// <param name="pickupFilePath">Path to the pickup WAV file.</param>
    /// <param name="flaggedSentences">CRX target sentences to match against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Matches sorted by pickup start time.</returns>
    public async Task<List<PickupMatch>> MatchPickupAsync(
        string pickupFilePath,
        IReadOnlyList<HydratedSentence> flaggedSentences,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(flaggedSentences);

        // 1. Decode pickup file
        var pickupBuffer = AudioProcessor.Decode(pickupFilePath);

        // 2. Detect silence to segment session recordings
        var silences = AudioProcessor.DetectSilence(pickupBuffer, new SilenceDetectOptions
        {
            NoiseDb = -45.0,
            MinimumDuration = TimeSpan.FromSeconds(1.5)
        });

        // 3. Derive speech segments from silence intervals
        var segments = DeriveSegments(pickupBuffer, silences);

        // 4. Process each segment through ASR and match
        var asrOptions = BuildAsrOptions();
        var matches = new List<PickupMatch>();

        foreach (var segment in segments)
        {
            ct.ThrowIfCancellationRequested();

            // a. Trim segment from pickup buffer
            var segBuffer = AudioProcessor.Trim(
                pickupBuffer,
                TimeSpan.FromSeconds(segment.StartSec),
                TimeSpan.FromSeconds(segment.EndSec));

            // b. Prepare for ASR (mono 16kHz)
            var asrReady = AsrAudioPreparer.PrepareForAsr(segBuffer);

            // c. Run ASR
            var asrResponse = await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct)
                .ConfigureAwait(false);

            // d. Extract full text from ASR tokens
            var recognizedText = ExtractFullText(asrResponse);
            if (string.IsNullOrWhiteSpace(recognizedText))
                continue;

            // e. Fuzzy match against flagged sentences
            var normalizedRecognized = NormalizeForMatch(recognizedText);

            double bestScore = 0;
            HydratedSentence? bestSentence = null;

            foreach (var sentence in flaggedSentences)
            {
                var normalizedTarget = NormalizeForMatch(sentence.BookText);
                var similarity = LevenshteinMetrics.Similarity(normalizedRecognized, normalizedTarget);

                if (similarity > bestScore)
                {
                    bestScore = similarity;
                    bestSentence = sentence;
                }
            }

            // f. Accept match if confidence exceeds threshold
            if (bestScore > MatchConfidenceThreshold && bestSentence != null)
            {
                matches.Add(new PickupMatch(
                    SentenceId: bestSentence.Id,
                    PickupStartSec: segment.StartSec,
                    PickupEndSec: segment.EndSec,
                    Confidence: bestScore,
                    RecognizedText: recognizedText));
            }
        }

        // 5. Return sorted by pickup start time
        return matches.OrderBy(m => m.PickupStartSec).ToList();
    }

    /// <summary>
    /// Simplified matching for individual pickup files (one per sentence).
    /// Runs ASR on the entire file and creates a match with the specified target sentence.
    /// </summary>
    /// <param name="pickupFilePath">Path to the pickup WAV file.</param>
    /// <param name="targetSentence">The sentence this pickup is intended to replace.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A single pickup match with confidence score.</returns>
    public async Task<PickupMatch> MatchSinglePickupAsync(
        string pickupFilePath,
        HydratedSentence targetSentence,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(targetSentence);

        // Decode and prepare for ASR
        var pickupBuffer = AudioProcessor.Decode(pickupFilePath);
        var asrReady = AsrAudioPreparer.PrepareForAsr(pickupBuffer);

        // Run ASR on entire file
        var asrOptions = BuildAsrOptions();
        var asrResponse = await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct)
            .ConfigureAwait(false);

        var recognizedText = ExtractFullText(asrResponse);
        var pickupDuration = (double)pickupBuffer.Length / pickupBuffer.SampleRate;

        // Compute confidence via Levenshtein for reference
        double confidence = 0;
        if (!string.IsNullOrWhiteSpace(recognizedText))
        {
            var normalizedRecognized = NormalizeForMatch(recognizedText);
            var normalizedTarget = NormalizeForMatch(targetSentence.BookText);
            confidence = LevenshteinMetrics.Similarity(normalizedRecognized, normalizedTarget);
        }

        return new PickupMatch(
            SentenceId: targetSentence.Id,
            PickupStartSec: 0,
            PickupEndSec: pickupDuration,
            Confidence: confidence,
            RecognizedText: recognizedText ?? string.Empty);
    }

    /// <summary>
    /// Derives speech segments from silence intervals. Segments are the gaps
    /// between detected silences (i.e., the non-silent portions of audio).
    /// Filters out segments shorter than <see cref="MinSegmentDurationSec"/>.
    /// </summary>
    private static List<(double StartSec, double EndSec)> DeriveSegments(
        AudioBuffer buffer,
        IReadOnlyList<SilenceInterval> silences)
    {
        var totalDuration = (double)buffer.Length / buffer.SampleRate;
        var segments = new List<(double StartSec, double EndSec)>();

        if (silences.Count == 0)
        {
            // No silence detected -- entire buffer is one segment
            if (totalDuration >= MinSegmentDurationSec)
                segments.Add((0, totalDuration));
            return segments;
        }

        // Before first silence
        var firstSilenceStart = silences[0].Start.TotalSeconds;
        if (firstSilenceStart >= MinSegmentDurationSec)
        {
            segments.Add((0, firstSilenceStart));
        }

        // Between silences
        for (int i = 0; i < silences.Count - 1; i++)
        {
            var gapStart = silences[i].End.TotalSeconds;
            var gapEnd = silences[i + 1].Start.TotalSeconds;
            var gapDuration = gapEnd - gapStart;

            if (gapDuration >= MinSegmentDurationSec)
            {
                segments.Add((gapStart, gapEnd));
            }
        }

        // After last silence
        var lastSilenceEnd = silences[^1].End.TotalSeconds;
        var trailingDuration = totalDuration - lastSilenceEnd;
        if (trailingDuration >= MinSegmentDurationSec)
        {
            segments.Add((lastSilenceEnd, totalDuration));
        }

        return segments;
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
    /// Normalizes text for fuzzy matching: lowercase, collapse whitespace, remove punctuation.
    /// </summary>
    private static string NormalizeForMatch(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.ToLowerInvariant().Trim();
        normalized = PunctuationRegex.Replace(normalized, " ");
        normalized = WhitespaceRegex.Replace(normalized, " ").Trim();
        return normalized;
    }

    /// <summary>
    /// Builds default ASR options using the environment-configured Whisper model.
    /// </summary>
    private static AsrOptions BuildAsrOptions()
    {
        var modelPath = AsrEngineConfig.ResolveModelPath(null);
        return new AsrOptions(
            ModelPath: modelPath,
            Language: "en",
            EnableWordTimestamps: true);
    }
}
