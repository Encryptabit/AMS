using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Runs ASR on pickup recording files and matches segmented pickup utterances to
/// CRX targets. Session-file pairing is deterministic and CRX-driven: MFA-refined
/// pickup segment ranges are assigned to CRX entries in ErrorNumber order.
/// </summary>
public class PickupMatchingService
{
    private const double LowConfidenceThreshold = 0.4;
    private const double TextSimilarityMinThreshold = 0.2;
    private const double MinSegmentDurationSec = 0.3;
    private const double DefaultUtteranceGapSec = 0.8;

    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BlazorWorkspace _workspace;
    private readonly PickupMfaRefinementService _mfaRefinement;

    public PickupMatchingService(BlazorWorkspace workspace, PickupMfaRefinementService mfaRefinement)
    {
        _workspace = workspace;
        _mfaRefinement = mfaRefinement;
    }

    /// <summary>
    /// Processes a pickup session recording using CRX-driven positional pairing.
    /// ASR + MFA run on the full WAV, then utterances are segmented by silence gaps
    /// and paired with CRX targets in ErrorNumber order.
    /// </summary>
    public async Task<List<PickupMatch>> MatchPickupCrxAsync(
        string pickupFilePath,
        IReadOnlyList<CrxPickupTarget> crxTargets,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(crxTargets);

        if (crxTargets.Count == 0)
        {
            throw new InvalidOperationException(
                "Pickup import requires CRX.json targets. No CRX targets were provided for deterministic pairing.");
        }

        // 1. ASR on full WAV (with named artifact cache)
        var asrResponse = TryReadNamedAsrCache(pickupFilePath);

        if (asrResponse == null)
        {
            var pickupBuffer = AudioProcessor.Decode(pickupFilePath);
            var asrReady = AsrAudioPreparer.PrepareForAsr(pickupBuffer);
            var asrOptions = await BuildAsrOptionsAsync(ct).ConfigureAwait(false);
            asrResponse = await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct)
                .ConfigureAwait(false);
            WriteNamedAsrCache(pickupFilePath, asrResponse);
        }

        // 2. MFA refinement
        asrResponse = await _mfaRefinement.RefineAsrTimingsAsync(
            pickupFilePath,
            asrResponse,
            ct).ConfigureAwait(false);
        WriteNamedMfaCache(asrResponse);

        // 3. Utterance segmentation: detect gaps using MFA-aware threshold (0.4s)
        //    since MFA refinement gives precise phone boundaries.
        var tokens = asrResponse.Tokens;
        if (tokens is not { Length: > 0 })
        {
            throw new InvalidOperationException(
                "Pickup import could not derive any MFA-refined pickup tokens from the session file.");
        }

        var segments = SegmentUtterances(tokens, utteranceGapThresholdSec: 0.4);
        var sortedTargets = crxTargets.OrderBy(t => t.ErrorNumber).ToList();
        return PairSegmentsToTargets(segments, sortedTargets);
    }

    /// <summary>
    /// Matches pickup segments to CRX targets using greedy best-first text similarity.
    /// Handles out-of-order recordings by pairing each segment with its best-matching
    /// target regardless of position. Uses Levenshtein similarity scoring.
    /// </summary>
    /// <remarks>
    /// Algorithm (greedy best-first assignment per research Pattern 7):
    /// 1. Compute similarity score for every segment × target pair
    /// 2. Pick the highest-confidence pair, assign it, remove both from pools
    /// 3. Repeat until best remaining score is below threshold or pools are empty
    /// 4. Remaining unmatched segments get zero-confidence entries
    ///
    /// Complexity is O(n² · m) which is negligible for typical pool sizes (5–30 items).
    /// </remarks>
    public static List<PickupMatch> MatchByTextSimilarity(
        IReadOnlyList<PickupSegment> segments,
        IReadOnlyList<CrxPickupTarget> targets,
        bool logWarnings = true)
    {
        if (segments.Count == 0 || targets.Count == 0)
            return new List<PickupMatch>();

        // Available indices
        var availableSegments = new HashSet<int>(Enumerable.Range(0, segments.Count));
        var availableTargets = new HashSet<int>(Enumerable.Range(0, targets.Count));

        var matches = new List<PickupMatch>();

        while (availableSegments.Count > 0 && availableTargets.Count > 0)
        {
            // Find the best (segment, target) pair among remaining
            double bestScore = -1;
            int bestSegIdx = -1;
            int bestTgtIdx = -1;

            foreach (var si in availableSegments)
            {
                var normalizedSegment = NormalizeForMatch(segments[si].TranscribedText);
                if (string.IsNullOrWhiteSpace(normalizedSegment))
                    continue;

                foreach (var ti in availableTargets)
                {
                    var normalizedTarget = NormalizeForMatch(targets[ti].ShouldBeText);
                    var score = LevenshteinMetrics.Similarity(normalizedSegment, normalizedTarget);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestSegIdx = si;
                        bestTgtIdx = ti;
                    }
                }
            }

            // Stop if no useful matches remain
            if (bestScore < TextSimilarityMinThreshold || bestSegIdx < 0 || bestTgtIdx < 0)
                break;

            var segment = segments[bestSegIdx];
            var target = targets[bestTgtIdx];
            var isLowConfidence = bestScore < LowConfidenceThreshold;

            if (isLowConfidence && logWarnings)
            {
                Log.Warn(
                    "Low confidence text match for error #{ErrorNumber} (sentence {SentenceId}): " +
                    "confidence={Confidence:F3}, expected=\"{Expected}\", got=\"{Got}\"",
                    target.ErrorNumber, target.SentenceId, bestScore,
                    target.ShouldBeText, segment.TranscribedText);
            }

            matches.Add(new PickupMatch(
                SentenceId: target.SentenceId,
                PickupStartSec: segment.StartSec,
                PickupEndSec: segment.EndSec,
                Confidence: bestScore,
                RecognizedText: segment.TranscribedText,
                ErrorNumber: target.ErrorNumber,
                IsLowConfidence: isLowConfidence));

            availableSegments.Remove(bestSegIdx);
            availableTargets.Remove(bestTgtIdx);
        }

        // Remaining unmatched segments
        foreach (var si in availableSegments)
        {
            var segment = segments[si];
            matches.Add(new PickupMatch(
                SentenceId: 0,
                PickupStartSec: segment.StartSec,
                PickupEndSec: segment.EndSec,
                Confidence: 0.0,
                RecognizedText: segment.TranscribedText,
                IsLowConfidence: true));
        }

        return matches;
    }

    /// <summary>
    /// Segments ASR tokens into utterances based on silence gaps.
    /// Short fragments are merged into neighboring segments rather than dropped.
    /// </summary>
    /// <param name="tokens">ASR tokens to segment.</param>
    /// <param name="utteranceGapThresholdSec">
    /// Minimum gap duration (seconds) to split utterances. Use 0.4 for MFA-refined
    /// timings (more precise word boundaries), or 0.8 (default) for raw ASR timings.
    /// </param>
    internal static List<PickupSegment> SegmentUtterances(
        AsrToken[] tokens,
        double utteranceGapThresholdSec = DefaultUtteranceGapSec)
    {
        var segments = new List<PickupSegment>();
        if (tokens.Length == 0)
            return segments;

        // MFA-refined tokens can occasionally be non-monotonic; normalize order first.
        var orderedTokens = tokens
            .Select((token, index) => (Token: token, Index: index))
            .OrderBy(x => x.Token.StartTime)
            .ThenBy(x => x.Index)
            .Select(x => x.Token)
            .ToArray();

        var tokenGroups = new List<List<AsrToken>>();
        var currentGroup = new List<AsrToken> { orderedTokens[0] };

        for (int i = 1; i < orderedTokens.Length; i++)
        {
            var previous = currentGroup[^1];
            var prevEnd = previous.StartTime + Math.Max(0.01, previous.Duration);
            var gap = orderedTokens[i].StartTime - prevEnd;

            if (gap >= utteranceGapThresholdSec)
            {
                tokenGroups.Add(currentGroup);
                currentGroup = new List<AsrToken>();
            }

            currentGroup.Add(orderedTokens[i]);
        }

        if (currentGroup.Count > 0)
            tokenGroups.Add(currentGroup);

        MergeShortTokenGroups(tokenGroups);

        foreach (var group in tokenGroups)
        {
            var (startSec, endSec, text, durationSec) = DescribeTokenGroup(group);
            if (string.IsNullOrWhiteSpace(text))
                continue;

            if (durationSec < MinSegmentDurationSec)
            {
                Log.Debug(
                    "Keeping isolated short segment ({Duration:F2}s < {Min:F1}s): \"{Text}\"",
                    durationSec, MinSegmentDurationSec, text);
            }

            segments.Add(new PickupSegment(startSec, endSec, text));
        }

        return segments;
    }

    private static void MergeShortTokenGroups(List<List<AsrToken>> tokenGroups)
    {
        if (tokenGroups.Count <= 1)
            return;

        int i = 0;
        while (i < tokenGroups.Count)
        {
            var (_, _, text, durationSec) = DescribeTokenGroup(tokenGroups[i]);
            if (durationSec >= MinSegmentDurationSec || string.IsNullOrWhiteSpace(text))
            {
                i++;
                continue;
            }

            var mergeTargetIndex = ChooseMergeTarget(tokenGroups, i);
            if (mergeTargetIndex < 0)
            {
                i++;
                continue;
            }

            Log.Debug(
                "Merging short segment ({Duration:F2}s < {Min:F1}s): \"{Text}\"",
                durationSec, MinSegmentDurationSec, text);

            if (mergeTargetIndex < i)
            {
                tokenGroups[mergeTargetIndex].AddRange(tokenGroups[i]);
            }
            else
            {
                tokenGroups[mergeTargetIndex].InsertRange(0, tokenGroups[i]);
            }

            tokenGroups.RemoveAt(i);
            i = Math.Max(0, i - 1);
        }
    }

    private static int ChooseMergeTarget(IReadOnlyList<List<AsrToken>> tokenGroups, int index)
    {
        if (tokenGroups.Count <= 1)
            return -1;

        if (index == 0)
            return 1;

        if (index == tokenGroups.Count - 1)
            return index - 1;

        var (_, prevEndSec, _, _) = DescribeTokenGroup(tokenGroups[index - 1]);
        var (currentStartSec, currentEndSec, _, _) = DescribeTokenGroup(tokenGroups[index]);
        var (nextStartSec, _, _, _) = DescribeTokenGroup(tokenGroups[index + 1]);

        var gapToPrevious = Math.Abs(currentStartSec - prevEndSec);
        var gapToNext = Math.Abs(nextStartSec - currentEndSec);
        return gapToNext <= gapToPrevious ? index + 1 : index - 1;
    }

    private static (double StartSec, double EndSec, string Text, double DurationSec) DescribeTokenGroup(
        IReadOnlyList<AsrToken> tokenGroup)
    {
        if (tokenGroup.Count == 0)
            return (0, 0, string.Empty, 0);

        var startSec = tokenGroup[0].StartTime;
        var endSec = tokenGroup[0].StartTime + Math.Max(0.01, tokenGroup[0].Duration);

        for (int i = 1; i < tokenGroup.Count; i++)
        {
            var token = tokenGroup[i];
            var tokenStart = token.StartTime;
            var tokenEnd = token.StartTime + Math.Max(0.01, token.Duration);
            if (tokenStart < startSec) startSec = tokenStart;
            if (tokenEnd > endSec) endSec = tokenEnd;
        }

        var text = string.Join(" ", tokenGroup.Select(t => t.Word)).Trim();
        var durationSec = Math.Max(0, endSec - startSec);
        return (startSec, endSec, text, durationSec);
    }

    /// <summary>
    /// Pairs utterance segments to CRX targets deterministically in ErrorNumber order.
    /// The MFA-refined pickup segment timing range becomes the pickup range owned by
    /// the corresponding CRX entry. Counts must match exactly.
    /// </summary>
    private static List<PickupMatch> PairSegmentsToTargets(
        List<PickupSegment> segments,
        List<CrxPickupTarget> targets)
    {
        if (segments.Count == 0 || targets.Count == 0)
            return new List<PickupMatch>();

        if (segments.Count != targets.Count)
        {
            throw new InvalidOperationException(
                $"Deterministic CRX pairing requires equal counts, but detected {segments.Count} pickup segments " +
                $"for {targets.Count} CRX targets. Re-import after fixing segmentation or CRX coverage.");
        }

        Log.Debug(
            "Pickup matching strategy selected: deterministic-crx-order (segments={Segments}, targets={Targets})",
            segments.Count,
            targets.Count);

        var matches = new List<PickupMatch>();

        for (int i = 0; i < targets.Count; i++)
        {
            var segment = segments[i];
            var target = targets[i];

            matches.Add(new PickupMatch(
                SentenceId: target.SentenceId,
                PickupStartSec: segment.StartSec,
                PickupEndSec: segment.EndSec,
                Confidence: 1.0,
                RecognizedText: segment.TranscribedText,
                ErrorNumber: target.ErrorNumber,
                IsLowConfidence: false));
        }

        return matches;
    }

    /// <summary>
    /// Simplified matching for individual pickup files (one per sentence).
    /// Runs ASR on the entire file and creates a match with the specified target sentence.
    /// </summary>
    public async Task<PickupMatch> MatchSinglePickupAsync(
        string pickupFilePath,
        HydratedSentence targetSentence,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(targetSentence);

        var pickupBuffer = AudioProcessor.Decode(pickupFilePath);
        var asrReady = AsrAudioPreparer.PrepareForAsr(pickupBuffer);

        var asrOptions = await BuildAsrOptionsAsync(ct).ConfigureAwait(false);
        var asrResponse = await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct)
            .ConfigureAwait(false);

        var recognizedText = ExtractFullText(asrResponse);
        var pickupDuration = (double)pickupBuffer.Length / pickupBuffer.SampleRate;

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
    /// Public for reuse by <see cref="PickupAssetService"/> and other callers.
    /// </summary>
    public static string ExtractFullText(AsrResponse response)
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
    /// Public for reuse by <see cref="PickupAssetService"/> and other callers.
    /// </summary>
    public static string NormalizeForMatch(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.ToLowerInvariant().Trim();
        normalized = PunctuationRegex.Replace(normalized, " ");
        normalized = WhitespaceRegex.Replace(normalized, " ").Trim();
        return normalized;
    }

    private static async Task<AsrOptions> BuildAsrOptionsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var (modelPath, _) = await AsrEngineConfig.ResolveModelPathAsync().ConfigureAwait(false);
        return new AsrOptions(
            ModelPath: modelPath,
            Language: "en",
            EnableWordTimestamps: true);
    }

    #region Named Artifact Cache

    private string GetPickupsDir()
    {
        var workDir = _workspace.WorkingDirectory
            ?? throw new InvalidOperationException("No working directory set.");
        var dir = Path.Combine(workDir, ".polish", "pickups");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private AsrResponse? TryReadNamedAsrCache(string pickupFilePath)
    {
        try
        {
            var cachePath = Path.Combine(GetPickupsDir(), "pickups.asr.json");
            if (!File.Exists(cachePath)) return null;

            var json = File.ReadAllText(cachePath);
            var wrapper = JsonSerializer.Deserialize<AsrCacheWrapper>(json);
            if (wrapper == null) return null;

            // Staleness check: pickup file identity
            var fi = new FileInfo(pickupFilePath);
            if (wrapper.PickupFilePath != fi.FullName ||
                wrapper.PickupFileSizeBytes != fi.Length ||
                wrapper.PickupFileModifiedUtc != fi.LastWriteTimeUtc)
            {
                return null;
            }

            return wrapper.AsrResponse;
        }
        catch
        {
            return null;
        }
    }

    private void WriteNamedAsrCache(string pickupFilePath, AsrResponse response)
    {
        try
        {
            var fi = new FileInfo(pickupFilePath);
            var wrapper = new AsrCacheWrapper(
                fi.FullName, fi.Length, fi.LastWriteTimeUtc, response);

            var cachePath = Path.Combine(GetPickupsDir(), "pickups.asr.json");
            var json = JsonSerializer.Serialize(wrapper, CacheJsonOptions);
            File.WriteAllText(cachePath, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write named ASR cache: {Message}", ex.Message);
        }
    }

    private void WriteNamedMfaCache(AsrResponse response)
    {
        try
        {
            var cachePath = Path.Combine(GetPickupsDir(), "pickups.asr.mfa.json");
            var json = JsonSerializer.Serialize(response, CacheJsonOptions);
            File.WriteAllText(cachePath, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write named MFA cache: {Message}", ex.Message);
        }
    }

    private sealed record AsrCacheWrapper(
        string PickupFilePath,
        long PickupFileSizeBytes,
        DateTime PickupFileModifiedUtc,
        AsrResponse AsrResponse);

    #endregion
}
