using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
/// Supports both multi-pickup session files (single-pass ASR with
/// token-gap segmentation) and individual pickup files (one per sentence).
/// Uses Whisper.NET in-process via <see cref="AsrProcessor"/> exclusively.
/// </summary>
public class PickupMatchingService
{
    private const double MinSegmentDurationSec = 0.3;
    private const double MatchConfidenceThreshold = 0.5;
    private const double TokenGapThresholdSec = 1.0;

    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly BlazorWorkspace _workspace;

    public PickupMatchingService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Processes a pickup recording (session or individual), runs ASR once
    /// on the entire file, derives segments from token gaps, and fuzzy-matches
    /// each to the best candidate in <paramref name="flaggedSentences"/>.
    /// Results are cached on disk for instant re-import.
    /// </summary>
    public async Task<List<PickupMatch>> MatchPickupAsync(
        string pickupFilePath,
        IReadOnlyList<HydratedSentence> flaggedSentences,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(flaggedSentences);

        // 1. Check ASR cache
        var asrResponse = TryReadAsrCache(pickupFilePath);

        if (asrResponse == null)
        {
            // 2. Decode, prepare, run ASR once on the entire file
            var pickupBuffer = AudioProcessor.Decode(pickupFilePath);
            var asrReady = AsrAudioPreparer.PrepareForAsr(pickupBuffer);
            var asrOptions = await BuildAsrOptionsAsync(ct).ConfigureAwait(false);
            asrResponse = await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct)
                .ConfigureAwait(false);

            // 3. Cache for instant re-import
            WriteAsrCache(pickupFilePath, asrResponse);
        }

        // 4. Derive speech segments from token gaps
        var segments = DeriveSegmentsFromTokens(asrResponse.Tokens);

        // 5. Fuzzy match each segment against flagged sentences
        var matches = new List<PickupMatch>();

        foreach (var segment in segments)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(segment.Text))
                continue;

            var normalizedRecognized = NormalizeForMatch(segment.Text);

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

            if (bestScore > MatchConfidenceThreshold && bestSentence != null)
            {
                matches.Add(new PickupMatch(
                    SentenceId: bestSentence.Id,
                    PickupStartSec: segment.StartSec,
                    PickupEndSec: segment.EndSec,
                    Confidence: bestScore,
                    RecognizedText: segment.Text));
            }
        }

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
        var asrOptions = await BuildAsrOptionsAsync(ct).ConfigureAwait(false);
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
    /// Derives speech segments from ASR token timestamps by splitting on gaps
    /// that exceed <see cref="TokenGapThresholdSec"/>. Each segment carries
    /// concatenated text from its tokens. Replaces silence detection + per-segment ASR.
    /// </summary>
    private static List<(double StartSec, double EndSec, string Text)> DeriveSegmentsFromTokens(
        AsrToken[] tokens)
    {
        var segments = new List<(double StartSec, double EndSec, string Text)>();

        if (tokens is not { Length: > 0 })
            return segments;

        var segStartSec = tokens[0].StartTime;
        var segEndSec = tokens[0].StartTime + tokens[0].Duration;
        var words = new List<string> { tokens[0].Word };

        for (int i = 1; i < tokens.Length; i++)
        {
            var token = tokens[i];
            var gap = token.StartTime - segEndSec;

            if (gap >= TokenGapThresholdSec)
            {
                // Flush current segment
                var duration = segEndSec - segStartSec;
                if (duration >= MinSegmentDurationSec)
                {
                    segments.Add((segStartSec, segEndSec, string.Join(" ", words).Trim()));
                }

                // Start new segment
                segStartSec = token.StartTime;
                words.Clear();
            }

            segEndSec = token.StartTime + token.Duration;
            words.Add(token.Word);
        }

        // Flush final segment
        var finalDuration = segEndSec - segStartSec;
        if (finalDuration >= MinSegmentDurationSec)
        {
            segments.Add((segStartSec, segEndSec, string.Join(" ", words).Trim()));
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

    #region ASR Cache

    private string GetCachePath(string pickupFilePath)
    {
        var hash = ComputeCacheKey(pickupFilePath);
        var workDir = _workspace.WorkingDirectory
            ?? throw new InvalidOperationException("No working directory set.");
        return Path.Combine(workDir, ".polish", "pickups", $"{hash}.asr.json");
    }

    private static string ComputeCacheKey(string filePath)
    {
        var fi = new FileInfo(filePath);
        var input = $"{fi.FullName}|{fi.Length}|{fi.LastWriteTimeUtc:O}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private AsrResponse? TryReadAsrCache(string pickupFilePath)
    {
        var cachePath = GetCachePath(pickupFilePath);
        if (!File.Exists(cachePath))
            return null;

        try
        {
            var json = File.ReadAllText(cachePath);
            return JsonSerializer.Deserialize<AsrResponse>(json);
        }
        catch
        {
            return null;
        }
    }

    private void WriteAsrCache(string pickupFilePath, AsrResponse response)
    {
        var cachePath = GetCachePath(pickupFilePath);
        var dir = Path.GetDirectoryName(cachePath)!;
        Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(response, CacheJsonOptions);
        File.WriteAllText(cachePath, json);
    }

    #endregion
}
