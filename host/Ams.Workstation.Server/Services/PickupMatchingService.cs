using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Services.Alignment;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Runs ASR on pickup recording files and matches segmented pickup utterances to
/// CRX targets. Session-file pairing is deterministic and CRX-driven: MFA-refined
/// pickup segment ranges are assigned to CRX entries in ErrorNumber order.
/// </summary>
public class PickupMatchingService
{
    private const double MinSegmentDurationSec = 0.3;
    private const int MaxSegmentsPerTarget = 6;

    private static readonly Regex PunctuationRegex = new(@"[^\w\s]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BlazorWorkspace _workspace;
    private readonly PickupMfaRefinementService _mfaRefinement;
    private readonly ChunkPlanningService _chunkPlanning = new();

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

        var pickupBuffer = AudioProcessor.Decode(pickupFilePath);
        var asrReady = AsrAudioPreparer.PrepareForAsr(pickupBuffer);
        var chunkPlan = _chunkPlanning.GeneratePlan(asrReady, pickupFilePath, new ChunkPlanningPolicy()
        {
            MaxChunkDuration = TimeSpan.FromSeconds(15),
            MinChunkDuration = TimeSpan.FromSeconds(5),
            MinSilenceDuration = TimeSpan.FromSeconds(2)
        });

        // 1. ASR on full WAV (with named artifact cache)
        var asrResponse = TryReadNamedAsrCache(pickupFilePath);

        if (asrResponse == null)
        {
            var asrOptions = await BuildAsrOptionsAsync(ct).ConfigureAwait(false);
            asrResponse = await TranscribePickupBufferAsync(asrReady, chunkPlan, asrOptions, ct)
                .ConfigureAwait(false);
            WriteNamedAsrCache(pickupFilePath, asrResponse);
        }

        // 2. MFA refinement
        asrResponse = await _mfaRefinement.RefineAsrTimingsAsync(
            pickupFilePath,
            asrReady,
            chunkPlan,
            asrResponse,
            ct).ConfigureAwait(false);
        WriteNamedMfaCache(asrResponse);

        var sortedTargets = crxTargets.OrderBy(t => t.ErrorNumber).ToList();

        // 3. Collapse ASR/MFA segments back to CRX-target utterances. Whisper sentence
        //    segments often split one pickup into multiple fragments, so deterministic
        //    pairing needs a partitioning pass rather than a raw count equality check.
        if (asrResponse.Segments is not { Length: > 0 })
        {
            throw new InvalidOperationException(
                "Pickup import could not derive any MFA-refined pickup segments from the session file.");
        }

        var segments = BuildDeterministicSegments(asrResponse.Segments, sortedTargets);
        if (segments.Count != crxTargets.Count)
        {
            throw new InvalidOperationException(
                $"Deterministic CRX pairing requires equal counts, but MFA produced {segments.Count} pickup segments for {crxTargets.Count} CRX targets.");
        }

        return PairSegmentsToTargets(segments, sortedTargets);
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

    internal static List<PickupSegment> BuildDeterministicSegments(
        IReadOnlyList<AsrSegment> segments,
        IReadOnlyList<CrxPickupTarget> targets)
    {
        var result = ConvertAsrSegments(segments);
        if (result.Count == 0 || targets.Count == 0)
        {
            return result;
        }

        if (result.Count == targets.Count)
        {
            return result;
        }

        if (result.Count < targets.Count)
        {
            return result;
        }

        return MergeSegmentsToTargets(result, targets);
    }

    private static List<PickupSegment> ConvertAsrSegments(IReadOnlyList<AsrSegment> segments)
    {
        var result = new List<PickupSegment>(segments.Count);
        foreach (var segment in segments)
        {
            if (segment.EndSec <= segment.StartSec)
            {
                continue;
            }

            var text = segment.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            result.Add(new PickupSegment(segment.StartSec, segment.EndSec, text));
        }

        return result;
    }

    private static List<PickupSegment> MergeSegmentsToTargets(
        IReadOnlyList<PickupSegment> segments,
        IReadOnlyList<CrxPickupTarget> targets)
    {
        var segmentCount = segments.Count;
        var targetCount = targets.Count;
        var costs = new double[segmentCount + 1, targetCount + 1];
        var previous = new int[segmentCount + 1, targetCount + 1];

        for (var i = 0; i <= segmentCount; i++)
        {
            for (var j = 0; j <= targetCount; j++)
            {
                costs[i, j] = double.PositiveInfinity;
                previous[i, j] = -1;
            }
        }

        costs[0, 0] = 0;

        for (var consumedSegments = 0; consumedSegments < segmentCount; consumedSegments++)
        {
            for (var consumedTargets = 0; consumedTargets < targetCount; consumedTargets++)
            {
                var baseCost = costs[consumedSegments, consumedTargets];
                if (double.IsPositiveInfinity(baseCost))
                {
                    continue;
                }

                var remainingTargetsAfterCurrent = targetCount - consumedTargets - 1;
                var maxEndExclusive = Math.Min(
                    segmentCount - remainingTargetsAfterCurrent,
                    consumedSegments + MaxSegmentsPerTarget);

                for (var endExclusive = consumedSegments + 1; endExclusive <= maxEndExclusive; endExclusive++)
                {
                    var candidate = MergeSegmentRange(segments, consumedSegments, endExclusive);
                    var assignmentCost = ComputeAssignmentCost(candidate.TranscribedText, targets[consumedTargets].ShouldBeText);
                    var totalCost = baseCost + assignmentCost;

                    if (totalCost < costs[endExclusive, consumedTargets + 1])
                    {
                        costs[endExclusive, consumedTargets + 1] = totalCost;
                        previous[endExclusive, consumedTargets + 1] = consumedSegments;
                    }
                }
            }
        }

        if (double.IsPositiveInfinity(costs[segmentCount, targetCount]))
        {
            return segments.ToList();
        }

        var merged = new List<PickupSegment>(targetCount);
        var segmentIndex = segmentCount;
        var targetIndex = targetCount;

        while (targetIndex > 0)
        {
            var startIndex = previous[segmentIndex, targetIndex];
            if (startIndex < 0)
            {
                return segments.ToList();
            }

            merged.Add(MergeSegmentRange(segments, startIndex, segmentIndex));
            segmentIndex = startIndex;
            targetIndex--;
        }

        merged.Reverse();
        return merged;
    }

    private static PickupSegment MergeSegmentRange(
        IReadOnlyList<PickupSegment> segments,
        int startIndex,
        int endExclusive)
    {
        var startSec = segments[startIndex].StartSec;
        var endSec = segments[endExclusive - 1].EndSec;
        var text = string.Join(" ", segments
            .Skip(startIndex)
            .Take(endExclusive - startIndex)
            .Select(s => s.TranscribedText)
            .Where(t => !string.IsNullOrWhiteSpace(t)))
            .Trim();

        return new PickupSegment(startSec, endSec, text);
    }

    private static double ComputeAssignmentCost(string candidateText, string targetText)
    {
        var candidateWords = SplitNormalizedWords(candidateText);
        var targetWords = SplitNormalizedWords(targetText);
        if (candidateWords.Count == 0 && targetWords.Count == 0)
        {
            return 0;
        }

        if (candidateWords.Count == 0 || targetWords.Count == 0)
        {
            return 1;
        }

        var distance = ComputeWordEditDistance(candidateWords, targetWords);
        var maxWordCount = Math.Max(candidateWords.Count, targetWords.Count);
        return distance / (double)maxWordCount;
    }

    private static IReadOnlyList<string> SplitNormalizedWords(string text)
    {
        var normalized = NormalizeForMatch(text);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Array.Empty<string>();
        }

        return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static int ComputeWordEditDistance(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        var dp = new int[left.Count + 1, right.Count + 1];

        for (var i = 0; i <= left.Count; i++)
        {
            dp[i, 0] = i;
        }

        for (var j = 0; j <= right.Count; j++)
        {
            dp[0, j] = j;
        }

        for (var i = 1; i <= left.Count; i++)
        {
            for (var j = 1; j <= right.Count; j++)
            {
                var substitutionCost = string.Equals(left[i - 1], right[j - 1], StringComparison.Ordinal)
                    ? 0
                    : 1;

                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + substitutionCost);
            }
        }

        return dp[left.Count, right.Count];
    }

    /// <summary>
    /// Individual-file pickup matching is intentionally unsupported.
    /// The pickup pipeline requires a single stitched pickup WAV and CRX-driven ordering.
    /// </summary>
    public async Task<PickupMatch> MatchSinglePickupAsync(
        string pickupFilePath,
        HydratedSentence targetSentence,
        CancellationToken ct)
    {
        await Task.CompletedTask;
        throw new NotSupportedException(
            "Individual-file pickup matching is not supported. Use a single stitched pickup WAV with CRX targets.");
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

    private async Task<AsrResponse> TranscribePickupBufferAsync(
        AudioBuffer asrReady,
        ChunkPlanDocument chunkPlan,
        AsrOptions asrOptions,
        CancellationToken ct)
    {
        if (chunkPlan.Chunks.Count <= 1)
        {
            return await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct).ConfigureAwait(false);
        }

        var totalDuration = asrReady.Length / (double)asrReady.SampleRate;
        Log.Debug(
            "Pickup ASR chunk-plan driven: {ChunkCount} chunks from {TotalDuration:F1}s audio",
            chunkPlan.Chunks.Count,
            totalDuration);

        var chunkResults = new List<(AsrResponse Response, double OffsetSec)>(chunkPlan.Chunks.Count);
        foreach (var entry in chunkPlan.Chunks)
        {
            ct.ThrowIfCancellationRequested();
            var slice = asrReady.Slice(entry.StartSample, entry.LengthSamples);
            var response = await AsrProcessor.TranscribeBufferAsync(slice, asrOptions, ct).ConfigureAwait(false);
            chunkResults.Add((Response: response, OffsetSec: entry.StartSec));
        }

        return MergeChunkResponses(chunkResults);
    }

    private static AsrResponse MergeChunkResponses(
        IReadOnlyList<(AsrResponse Response, double OffsetSec)> chunks)
    {
        var allTokens = new List<AsrToken>();
        var allSegments = new List<AsrSegment>();
        string? modelVersion = null;
        double lastTokenEnd = 0;
        double lastSegmentEnd = 0;

        var ordered = chunks.Count <= 1 ? chunks : chunks.OrderBy(c => c.OffsetSec).ToList();
        foreach (var (response, offsetSec) in ordered)
        {
            modelVersion ??= response.ModelVersion;

            if (response.Tokens is { Length: > 0 })
            {
                foreach (var token in response.Tokens)
                {
                    var adjustedStart = Math.Max(token.StartTime + offsetSec, lastTokenEnd);
                    var adjustedDuration = token.Duration;
                    allTokens.Add(new AsrToken(adjustedStart, adjustedDuration, token.Word));
                    lastTokenEnd = adjustedStart + Math.Max(0, adjustedDuration);
                }
            }

            if (response.Segments is { Length: > 0 })
            {
                foreach (var segment in response.Segments)
                {
                    var adjustedStart = Math.Max(segment.StartSec + offsetSec, lastSegmentEnd);
                    var adjustedEnd = Math.Max(segment.EndSec + offsetSec, adjustedStart);
                    allSegments.Add(new AsrSegment(adjustedStart, adjustedEnd, segment.Text));
                    lastSegmentEnd = adjustedEnd;
                }
            }
        }

        return new AsrResponse(modelVersion ?? "whisper", allTokens.ToArray(), allSegments.ToArray());
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
