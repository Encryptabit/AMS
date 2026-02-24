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
/// CRX-guided segmentation) and individual pickup files (one per sentence).
/// Uses Whisper.NET in-process via <see cref="AsrProcessor"/> exclusively.
/// </summary>
public class PickupMatchingService
{
    private const double MatchConfidenceThreshold = 0.5;

    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly BlazorWorkspace _workspace;
    private readonly PickupMfaRefinementService _mfaRefinement;

    public PickupMatchingService(BlazorWorkspace workspace, PickupMfaRefinementService mfaRefinement)
    {
        _workspace = workspace;
        _mfaRefinement = mfaRefinement;
    }

    /// <summary>
    /// Processes a pickup session recording by running ASR once on the entire
    /// file, then using the flagged CRX sentences to locate each sentence's
    /// token span in the ASR output. Sentences are matched sequentially
    /// (narrator records them in CRX order). Results are cached on disk.
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

        // 4. Use CRX sentences to find each sentence's token span
        var tokens = asrResponse.Tokens;
        var matches = new List<PickupMatch>();

        if (tokens is not { Length: > 0 } || flaggedSentences.Count == 0)
            return matches;

        // Process sentences in order — narrator records them sequentially
        int searchFrom = 0;

        foreach (var sentence in flaggedSentences)
        {
            ct.ThrowIfCancellationRequested();

            var normalizedTarget = NormalizeForMatch(sentence.BookText);
            var targetWords = normalizedTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (targetWords.Length == 0) continue;

            // Search for the best contiguous token window starting from searchFrom
            var (bestStart, bestEnd, bestScore) = FindBestTokenSpan(
                tokens, searchFrom, targetWords.Length, normalizedTarget);

            if (bestScore > MatchConfidenceThreshold && bestStart >= 0)
            {
                var startSec = tokens[bestStart].StartTime;
                var lastToken = tokens[bestEnd - 1];
                var endSec = lastToken.StartTime + lastToken.Duration;
                var recognizedText = string.Join(" ", tokens[bestStart..bestEnd].Select(t => t.Word));

                matches.Add(new PickupMatch(
                    SentenceId: sentence.Id,
                    PickupStartSec: startSec,
                    PickupEndSec: endSec,
                    Confidence: bestScore,
                    RecognizedText: recognizedText.Trim()));

                searchFrom = bestEnd;
            }
        }

        // Refine ASR timings with MFA forced alignment
        if (matches.Count > 0)
        {
            matches = (await _mfaRefinement.RefineWithMfaAsync(
                pickupFilePath, matches, flaggedSentences, ct)
                .ConfigureAwait(false)).ToList();
        }

        return matches.OrderBy(m => m.PickupStartSec).ToList();
    }

    /// <summary>
    /// Finds the contiguous token span starting at or after <paramref name="fromIndex"/>
    /// that best matches <paramref name="normalizedTarget"/>. Tries window sizes
    /// around the expected word count to handle ASR word-splitting variance.
    /// </summary>
    private static (int Start, int End, double Score) FindBestTokenSpan(
        AsrToken[] tokens, int fromIndex, int expectedWords, string normalizedTarget)
    {
        int bestStart = -1, bestEnd = -1;
        double bestScore = 0;

        // Allow window sizes from 60% to 150% of expected word count
        int minWindow = Math.Max(1, (int)(expectedWords * 0.6));
        int maxWindow = Math.Min(tokens.Length - fromIndex, (int)(expectedWords * 1.5) + 2);

        // Limit how far ahead we search (don't scan the entire remaining stream)
        int maxStartOffset = Math.Min(tokens.Length - fromIndex, expectedWords * 2 + 10);

        for (int offset = 0; offset < maxStartOffset; offset++)
        {
            int start = fromIndex + offset;

            for (int windowSize = minWindow; windowSize <= maxWindow; windowSize++)
            {
                int end = start + windowSize;
                if (end > tokens.Length) break;

                var windowText = string.Join(" ", tokens[start..end].Select(t => t.Word));
                var normalizedWindow = NormalizeForMatch(windowText);
                var score = LevenshteinMetrics.Similarity(normalizedWindow, normalizedTarget);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestStart = start;
                    bestEnd = end;
                }

                // Early exit: very high confidence match found
                if (score > 0.92) return (bestStart, bestEnd, bestScore);
            }
        }

        return (bestStart, bestEnd, bestScore);
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
