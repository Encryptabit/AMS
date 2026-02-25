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
/// Runs ASR on pickup recording files and matches recognized utterances to
/// CRX target sentences using positional pairing (CRX order drives the match).
/// Levenshtein similarity is used as a confidence metric, not a filter.
/// </summary>
public class PickupMatchingService
{
    private const double LowConfidenceThreshold = 0.4;
    private const double MinSegmentDurationSec = 0.3;
    private const double UtteranceGapSec = 0.8;

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
            return new List<PickupMatch>();

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

        // 3. Utterance segmentation: detect gaps >= 0.8s between MFA-refined tokens
        var tokens = asrResponse.Tokens;
        if (tokens is not { Length: > 0 })
            return new List<PickupMatch>();

        var segments = SegmentUtterances(tokens);

        // 4. CRX positional pairing
        var sortedTargets = crxTargets.OrderBy(t => t.ErrorNumber).ToList();
        var matches = PairSegmentsToTargets(segments, sortedTargets);

        return matches;
    }

    /// <summary>
    /// Segments ASR tokens into utterances based on silence gaps.
    /// </summary>
    private static List<PickupSegment> SegmentUtterances(AsrToken[] tokens)
    {
        var segments = new List<PickupSegment>();
        if (tokens.Length == 0) return segments;

        var segmentStart = tokens[0].StartTime;
        var segmentTokens = new List<AsrToken> { tokens[0] };

        for (int i = 1; i < tokens.Length; i++)
        {
            var prevEnd = tokens[i - 1].StartTime + tokens[i - 1].Duration;
            var gap = tokens[i].StartTime - prevEnd;

            if (gap >= UtteranceGapSec)
            {
                // End current segment
                var lastToken = segmentTokens[^1];
                var segmentEnd = lastToken.StartTime + lastToken.Duration;
                var text = string.Join(" ", segmentTokens.Select(t => t.Word)).Trim();
                var duration = segmentEnd - segmentStart;

                if (duration >= MinSegmentDurationSec && !string.IsNullOrWhiteSpace(text))
                {
                    segments.Add(new PickupSegment(segmentStart, segmentEnd, text));
                }
                else if (duration < MinSegmentDurationSec)
                {
                    Log.Debug(
                        "Skipping short segment ({Duration:F2}s < {Min:F1}s): \"{Text}\"",
                        duration, MinSegmentDurationSec, text);
                }

                // Start new segment
                segmentStart = tokens[i].StartTime;
                segmentTokens.Clear();
            }

            segmentTokens.Add(tokens[i]);
        }

        // Final segment
        if (segmentTokens.Count > 0)
        {
            var lastToken = segmentTokens[^1];
            var segmentEnd = lastToken.StartTime + lastToken.Duration;
            var text = string.Join(" ", segmentTokens.Select(t => t.Word)).Trim();
            var duration = segmentEnd - segmentStart;

            if (duration >= MinSegmentDurationSec && !string.IsNullOrWhiteSpace(text))
            {
                segments.Add(new PickupSegment(segmentStart, segmentEnd, text));
            }
        }

        return segments;
    }

    /// <summary>
    /// Pairs utterance segments to CRX targets using positional alignment.
    /// When there are more segments than targets, finds the best starting offset
    /// by maximizing total confidence.
    /// </summary>
    private static List<PickupMatch> PairSegmentsToTargets(
        List<PickupSegment> segments,
        List<CrxPickupTarget> targets)
    {
        if (segments.Count == 0 || targets.Count == 0)
            return new List<PickupMatch>();

        // Find best starting offset when segment count differs from target count
        int bestOffset = 0;
        double bestTotalConfidence = -1;

        int maxOffset = Math.Max(0, segments.Count - targets.Count);
        for (int offset = 0; offset <= maxOffset; offset++)
        {
            double totalConfidence = 0;
            int pairCount = Math.Min(targets.Count, segments.Count - offset);

            for (int i = 0; i < pairCount; i++)
            {
                var segment = segments[offset + i];
                var target = targets[i];
                var normalizedSegment = NormalizeForMatch(segment.TranscribedText);
                var normalizedTarget = NormalizeForMatch(target.ShouldBeText);
                totalConfidence += LevenshteinMetrics.Similarity(normalizedSegment, normalizedTarget);
            }

            if (totalConfidence > bestTotalConfidence)
            {
                bestTotalConfidence = totalConfidence;
                bestOffset = offset;
            }
        }

        // Pair using best offset
        var matches = new List<PickupMatch>();
        int pairsToMake = Math.Min(targets.Count, segments.Count - bestOffset);

        if (pairsToMake < targets.Count)
        {
            Log.Warn(
                "Fewer segments ({Segments}) than targets ({Targets}); {Unpaired} targets will be unmatched",
                segments.Count, targets.Count, targets.Count - pairsToMake);
        }

        for (int i = 0; i < pairsToMake; i++)
        {
            var segment = segments[bestOffset + i];
            var target = targets[i];

            var normalizedSegment = NormalizeForMatch(segment.TranscribedText);
            var normalizedTarget = NormalizeForMatch(target.ShouldBeText);
            var confidence = LevenshteinMetrics.Similarity(normalizedSegment, normalizedTarget);
            var isLowConfidence = confidence < LowConfidenceThreshold;

            if (isLowConfidence)
            {
                Log.Warn(
                    "Low confidence match for error #{ErrorNumber} (sentence {SentenceId}): " +
                    "confidence={Confidence:F3}, expected=\"{Expected}\", got=\"{Got}\"",
                    target.ErrorNumber, target.SentenceId, confidence,
                    target.ShouldBeText, segment.TranscribedText);
            }

            matches.Add(new PickupMatch(
                SentenceId: target.SentenceId,
                PickupStartSec: segment.StartSec,
                PickupEndSec: segment.EndSec,
                Confidence: confidence,
                RecognizedText: segment.TranscribedText,
                ErrorNumber: target.ErrorNumber,
                IsLowConfidence: isLowConfidence));
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
    /// </summary>
    internal static string ExtractFullText(AsrResponse response)
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
    internal static string NormalizeForMatch(string text)
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
