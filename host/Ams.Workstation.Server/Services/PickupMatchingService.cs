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
    // Tightened threshold to avoid false positives from single-token overlap.
    private const double MatchConfidenceThreshold = 0.62;

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
        }

        // 3. Refine ASR token timings with full-file MFA and persist refined ASR cache
        asrResponse = await _mfaRefinement.RefineAsrTimingsAsync(
            pickupFilePath,
            asrResponse,
            ct).ConfigureAwait(false);
        WriteAsrCache(pickupFilePath, asrResponse);

        // 4. Check final match results cache (keyed to refined ASR content + sentence context)
        var cachedMatches = TryReadMatchCache(pickupFilePath, asrResponse, flaggedSentences);
        if (cachedMatches != null)
            return cachedMatches;

        // 5. Use CRX sentences to find each sentence's token span
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
            var (bestStart, bestEnd, bestScore, bestSharedWords) = FindBestTokenSpan(
                tokens, searchFrom, targetWords, normalizedTarget);

            if (bestStart >= 0 && IsAcceptableMatch(bestScore, targetWords.Length, bestSharedWords))
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
            else if (bestStart >= 0)
            {
                Log.Debug(
                    "Rejected weak pickup match for sentence {SentenceId}: score={Score:F3}, shared={Shared}/{TargetWords}",
                    sentence.Id,
                    bestScore,
                    bestSharedWords,
                    targetWords.Length);
            }
        }

        var result = matches.OrderBy(m => m.PickupStartSec).ToList();

        // 6. Cache final results for instant re-import
        if (result.Count > 0)
            WriteMatchCache(pickupFilePath, asrResponse, flaggedSentences, result);

        return result;
    }

    /// <summary>
    /// Finds the contiguous token span starting at or after <paramref name="fromIndex"/>
    /// that best matches <paramref name="normalizedTarget"/>. Tries window sizes
    /// around the expected word count to handle ASR word-splitting variance.
    /// </summary>
    private static (int Start, int End, double Score, int SharedWords) FindBestTokenSpan(
        AsrToken[] tokens, int fromIndex, string[] targetWords, string normalizedTarget)
    {
        var expectedWords = targetWords.Length;
        var targetWordSet = targetWords.ToHashSet(StringComparer.Ordinal);

        int bestStart = -1, bestEnd = -1, bestSharedWords = 0;
        double bestScore = 0;

        // Allow window sizes from 60% to 150% of expected word count
        int minWindow = Math.Max(1, (int)(expectedWords * 0.6));
        int maxWindow = Math.Min(tokens.Length - fromIndex, (int)(expectedWords * 1.5) + 2);

        // Scan the entire remaining token stream so late-session pickups are discoverable.
        int maxStartOffset = tokens.Length - fromIndex;

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
                var sharedWords = CountSharedWords(normalizedWindow, targetWordSet);

                if (score > bestScore || (Math.Abs(score - bestScore) < 1e-9 && sharedWords > bestSharedWords))
                {
                    bestScore = score;
                    bestStart = start;
                    bestEnd = end;
                    bestSharedWords = sharedWords;
                }

                // Early exit: very high confidence + lexical overlap.
                if (score > 0.95 && sharedWords >= GetMinimumSharedWords(expectedWords))
                    return (bestStart, bestEnd, bestScore, bestSharedWords);
            }
        }

        return (bestStart, bestEnd, bestScore, bestSharedWords);
    }

    private static bool IsAcceptableMatch(double score, int targetWordCount, int sharedWords)
    {
        if (score <= MatchConfidenceThreshold)
            return false;

        var minSharedWords = GetMinimumSharedWords(targetWordCount);
        return sharedWords >= minSharedWords;
    }

    private static int GetMinimumSharedWords(int targetWordCount)
    {
        if (targetWordCount <= 1) return 1;
        if (targetWordCount <= 4) return 2;
        return Math.Max(2, (int)Math.Ceiling(targetWordCount * 0.5));
    }

    private static int CountSharedWords(string normalizedWindow, HashSet<string> targetWordSet)
    {
        var uniqueWindowWords = normalizedWindow
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);

        int shared = 0;
        foreach (var word in uniqueWindowWords)
        {
            if (targetWordSet.Contains(word))
                shared++;
        }

        return shared;
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

    #region Match Results Cache (ASR + MFA combined)

    /// <summary>
    /// Computes a cache key for final match results using pickup file identity,
    /// refined ASR token stream, and flagged sentence context.
    /// </summary>
    private static string ComputeMatchCacheKey(
        string pickupFilePath,
        AsrResponse asrResponse,
        IReadOnlyList<HydratedSentence> sentences)
    {
        var fi = new FileInfo(pickupFilePath);
        var sb = new StringBuilder();
        sb.Append(fi.FullName).Append('|').Append(fi.Length).Append('|').Append(fi.LastWriteTimeUtc.ToString("O"));
        sb.Append('|').Append(asrResponse.ModelVersion);
        foreach (var token in asrResponse.Tokens)
        {
            sb.Append('|')
                .Append(NormalizeForMatch(token.Word))
                .Append('@')
                .Append(token.StartTime.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
                .Append(',')
                .Append(token.Duration.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
        }
        sb.Append('|');
        sb.Append(string.Join(":", sentences.Select(s => s.Id)));
        foreach (var sentence in sentences)
        {
            sb.Append('|').Append(NormalizeForMatch(sentence.BookText));
        }
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private List<PickupMatch>? TryReadMatchCache(
        string pickupFilePath,
        AsrResponse asrResponse,
        IReadOnlyList<HydratedSentence> sentences)
    {
        var workDir = _workspace.WorkingDirectory;
        if (workDir == null) return null;

        var hash = ComputeMatchCacheKey(pickupFilePath, asrResponse, sentences);
        var cachePath = Path.Combine(workDir, ".polish", "pickups", $"{hash}.matches.json");
        if (!File.Exists(cachePath)) return null;

        try
        {
            var json = File.ReadAllText(cachePath);
            var matches = JsonSerializer.Deserialize<List<PickupMatch>>(json);
            if (matches is not { Count: > 0 })
                return null;

            var sentenceLookup = sentences.ToDictionary(s => s.Id);
            if (!AreCachedMatchesPlausible(matches, sentenceLookup))
            {
                Log.Debug("Ignoring implausible pickup match cache at {Path}", cachePath);
                return null;
            }

            return matches;
        }
        catch
        {
            return null;
        }
    }

    private void WriteMatchCache(
        string pickupFilePath,
        AsrResponse asrResponse,
        IReadOnlyList<HydratedSentence> sentences,
        List<PickupMatch> matches)
    {
        var workDir = _workspace.WorkingDirectory;
        if (workDir == null) return;

        try
        {
            var hash = ComputeMatchCacheKey(pickupFilePath, asrResponse, sentences);
            var dir = Path.Combine(workDir, ".polish", "pickups");
            Directory.CreateDirectory(dir);
            var cachePath = Path.Combine(dir, $"{hash}.matches.json");
            var json = JsonSerializer.Serialize(matches, CacheJsonOptions);
            File.WriteAllText(cachePath, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write match cache: {Message}", ex.Message);
        }
    }

    #endregion

    #region ASR Cache

    private static bool AreCachedMatchesPlausible(
        IReadOnlyList<PickupMatch> matches,
        Dictionary<int, HydratedSentence> sentenceLookup)
    {
        foreach (var match in matches)
        {
            if (!sentenceLookup.TryGetValue(match.SentenceId, out var sentence))
                return false;

            if (!IsPlausibleMatchDuration(match, sentence))
                return false;
        }

        return true;
    }

    private static bool IsPlausibleMatchDuration(PickupMatch match, HydratedSentence sentence)
    {
        var duration = match.PickupEndSec - match.PickupStartSec;
        if (duration <= 0.1)
            return false;

        var targetWords = NormalizeForMatch(sentence.BookText)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        // Heuristic guardrail: spoken sentence duration should roughly track word count.
        // Allows generous headroom for pauses but rejects obviously wrong whole-file spans.
        var maxDuration = Math.Max(12.0, targetWords * 1.5);
        return duration <= maxDuration;
    }

    private string GetAsrCachePath(string pickupFilePath)
    {
        var hash = ComputeAsrCacheKey(pickupFilePath);
        var workDir = _workspace.WorkingDirectory
            ?? throw new InvalidOperationException("No working directory set.");
        return Path.Combine(workDir, ".polish", "pickups", $"{hash}.asr.json");
    }

    private static string ComputeAsrCacheKey(string filePath)
    {
        var fi = new FileInfo(filePath);
        var input = $"{fi.FullName}|{fi.Length}|{fi.LastWriteTimeUtc:O}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private AsrResponse? TryReadAsrCache(string pickupFilePath)
    {
        var cachePath = GetAsrCachePath(pickupFilePath);
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
        var cachePath = GetAsrCachePath(pickupFilePath);
        var dir = Path.GetDirectoryName(cachePath)!;
        Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(response, CacheJsonOptions);
        File.WriteAllText(cachePath, json);
    }

    #endregion
}
