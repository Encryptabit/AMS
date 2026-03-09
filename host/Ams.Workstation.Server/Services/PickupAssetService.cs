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
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Unified pickup import service that normalizes both session-file segments
/// and individual WAV files into <see cref="PickupAsset"/> records with disk caching.
/// Auto-detects source type (directory of individual files vs single session file)
/// and delegates to the appropriate import pipeline.
/// </summary>
public class PickupAssetService
{
    private const string CrxFingerprintVersion = "pickup-matching-v2";

    /// <summary>
    /// Confidence threshold below which individual-file imports are considered unmatched.
    /// </summary>
    private const double IndividualFileUnmatchedThreshold = 0.2;

    /// <summary>
    /// Confidence threshold below which session-file segment imports are considered unmatched.
    /// </summary>
    private const double SessionSegmentUnmatchedThreshold = 0.3;

    /// <summary>
    /// MFA-aware gap threshold for utterance segmentation (seconds).
    /// MFA phone boundaries give more precise word endpoints, so shorter silences
    /// between them are legitimate utterance breaks.
    /// </summary>
    private const double MfaAwareGapThresholdSec = 0.4;

    private static readonly Regex ErrorNumberPrefixRegex =
        new(@"^(?:error|err)[_\-]?(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DirectNumberRegex =
        new(@"^(\d+)$", RegexOptions.Compiled);

    private static readonly Regex TrailingDigitsRegex =
        new(@"(\d+)\s*$", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly PickupMatchingService _pickupMatching;
    private readonly BlazorWorkspace _workspace;

    public PickupAssetService(PickupMatchingService pickupMatching, BlazorWorkspace workspace)
    {
        _pickupMatching = pickupMatching;
        _workspace = workspace;
    }

    /// <summary>
    /// Auto-detects source type and imports pickup assets accordingly.
    /// If <paramref name="sourcePath"/> is a directory, delegates to <see cref="ImportFromFolderAsync"/>.
    /// If it is a single WAV file, delegates to <see cref="ImportFromSessionFileAsync"/>.
    /// </summary>
    /// <returns>Tuple of (Matched, Unmatched) pickup assets.</returns>
    public async Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)> ImportAsync(
        string sourcePath,
        IReadOnlyList<CrxPickupTarget> crxTargets,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(crxTargets);

        if (Directory.Exists(sourcePath))
            return await ImportFromFolderAsync(sourcePath, crxTargets, ct).ConfigureAwait(false);

        if (File.Exists(sourcePath))
            return await ImportFromSessionFileAsync(sourcePath, crxTargets, ct).ConfigureAwait(false);

        throw new FileNotFoundException($"Source path does not exist: '{sourcePath}'");
    }

    /// <summary>
    /// Imports individual WAV files from a folder, matching each by filename pattern
    /// to CRX error numbers. Runs ASR on each file for text confidence scoring.
    /// </summary>
    public async Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)> ImportFromFolderAsync(
        string folderPath,
        IReadOnlyList<CrxPickupTarget> crxTargets,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);
        ArgumentNullException.ThrowIfNull(crxTargets);

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder does not exist: '{folderPath}'");

        var wavFiles = Directory.GetFiles(folderPath, "*.wav")
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (wavFiles.Count == 0)
            return (Array.Empty<PickupAsset>(), Array.Empty<PickupAsset>());

        // Build CRX lookup by error number
        var targetsByErrorNumber = crxTargets
            .GroupBy(t => t.ErrorNumber)
            .ToDictionary(g => g.Key, g => g.First());

        var matched = new List<PickupAsset>();
        var unmatched = new List<PickupAsset>();
        var now = DateTime.UtcNow;

        foreach (var wavFile in wavFiles)
        {
            ct.ThrowIfCancellationRequested();

            var fileName = Path.GetFileNameWithoutExtension(wavFile);
            var errorNumber = TryExtractErrorNumber(fileName);

            // Determine file duration
            var fileBuffer = AudioProcessor.Decode(wavFile);
            var fileDuration = (double)fileBuffer.Length / fileBuffer.SampleRate;

            // Run ASR for transcribed text
            var asrReady = AsrAudioPreparer.PrepareForAsr(fileBuffer);
            var asrOptions = await BuildAsrOptionsAsync(ct).ConfigureAwait(false);
            var asrResponse = await AsrProcessor.TranscribeBufferAsync(asrReady, asrOptions, ct)
                .ConfigureAwait(false);
            var transcribedText = PickupMatchingService.ExtractFullText(asrResponse);

            // Try to find matching CRX target
            CrxPickupTarget? matchedTarget = null;
            double confidence = 0;
            if (errorNumber.HasValue && targetsByErrorNumber.TryGetValue(errorNumber.Value, out var target))
            {
                matchedTarget = target;
                if (!string.IsNullOrWhiteSpace(transcribedText))
                {
                    var normalizedAsr = PickupMatchingService.NormalizeForMatch(transcribedText);
                    var normalizedShouldBe = PickupMatchingService.NormalizeForMatch(target.ShouldBeText);
                    confidence = LevenshteinMetrics.Similarity(normalizedAsr, normalizedShouldBe);
                }
            }

            var asset = new PickupAsset(
                Id: Guid.NewGuid().ToString("N"),
                SourceType: PickupSourceType.IndividualFile,
                SourceFilePath: wavFile,
                TrimStartSec: 0,
                TrimEndSec: fileDuration,
                TranscribedText: transcribedText,
                Confidence: confidence,
                MatchedErrorNumber: matchedTarget != null ? errorNumber : null,
                MatchedSentenceId: matchedTarget?.SentenceId,
                MatchedChapterStem: matchedTarget?.ChapterStem,
                ImportedAtUtc: now);

            if (asset.MatchedErrorNumber == null || asset.Confidence < IndividualFileUnmatchedThreshold)
                unmatched.Add(asset);
            else
                matched.Add(asset);
        }

        return (matched, unmatched);
    }

    /// <summary>
    /// Imports a session recording file by running the enhanced matching pipeline
    /// (ASR + MFA + segmentation) and wrapping results as <see cref="PickupAsset"/> records.
    /// Uses disk cache to avoid redundant processing.
    /// </summary>
    public async Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)> ImportFromSessionFileAsync(
        string sessionFilePath,
        IReadOnlyList<CrxPickupTarget> crxTargets,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionFilePath);
        ArgumentNullException.ThrowIfNull(crxTargets);

        if (!File.Exists(sessionFilePath))
            throw new FileNotFoundException($"Session file does not exist: '{sessionFilePath}'");

        if (crxTargets.Count == 0)
            return (Array.Empty<PickupAsset>(), Array.Empty<PickupAsset>());

        var fi = new FileInfo(sessionFilePath);
        var crxFingerprint = ComputeCrxFingerprint(crxTargets);

        // Check cache
        var cached = TryReadAssetCache();
        if (cached != null &&
            cached.SourceFilePath == fi.FullName &&
            cached.SourceFileSizeBytes == fi.Length &&
            cached.SourceFileModifiedUtc == fi.LastWriteTimeUtc &&
            cached.CrxTargetsFingerprint == crxFingerprint)
        {
            return SplitMatchedUnmatched(cached.Assets, SessionSegmentUnmatchedThreshold);
        }

        // Run the matching pipeline (delegates to PickupMatchingService)
        var matches = await _pickupMatching.MatchPickupCrxAsync(sessionFilePath, crxTargets, ct)
            .ConfigureAwait(false);

        // Build a lookup for CRX targets by error number
        var targetsByErrorNumber = crxTargets
            .GroupBy(t => t.ErrorNumber)
            .ToDictionary(g => g.Key, g => g.First());

        var assets = new List<PickupAsset>();
        var now = DateTime.UtcNow;

        foreach (var match in matches)
        {
            CrxPickupTarget? target = null;
            if (match.ErrorNumber.HasValue)
                targetsByErrorNumber.TryGetValue(match.ErrorNumber.Value, out target);

            assets.Add(new PickupAsset(
                Id: Guid.NewGuid().ToString("N"),
                SourceType: PickupSourceType.SessionSegment,
                SourceFilePath: fi.FullName,
                TrimStartSec: match.PickupStartSec,
                TrimEndSec: match.PickupEndSec,
                TranscribedText: match.RecognizedText,
                Confidence: match.Confidence,
                MatchedErrorNumber: match.ErrorNumber,
                MatchedSentenceId: target?.SentenceId ?? match.SentenceId,
                MatchedChapterStem: target?.ChapterStem,
                ImportedAtUtc: now));
        }

        // Persist cache
        var cache = new PickupAssetCache(
            SourceFilePath: fi.FullName,
            SourceFileSizeBytes: fi.Length,
            SourceFileModifiedUtc: fi.LastWriteTimeUtc,
            CrxTargetsFingerprint: crxFingerprint,
            Assets: assets,
            ProcessedAtUtc: now);
        WriteAssetCache(cache);

        return SplitMatchedUnmatched(assets, SessionSegmentUnmatchedThreshold);
    }

    /// <summary>
    /// Extracts an error number from a pickup filename.
    /// Supports patterns: "NNN" (direct), "error_NNN"/"err_NNN" (prefix),
    /// trailing digits fallback (e.g., "Chapter3_Error001" → 1).
    /// </summary>
    internal static int? TryExtractErrorNumber(string fileNameWithoutExtension)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            return null;

        var name = fileNameWithoutExtension.Trim();

        // Direct number: "001", "42"
        var directMatch = DirectNumberRegex.Match(name);
        if (directMatch.Success && int.TryParse(directMatch.Groups[1].Value, out var directNum))
            return directNum;

        // Prefix: "error_5", "err_5", "error-5", "err-5", "error5"
        var prefixMatch = ErrorNumberPrefixRegex.Match(name);
        if (prefixMatch.Success && int.TryParse(prefixMatch.Groups[1].Value, out var prefixNum))
            return prefixNum;

        // Trailing digits fallback: "Chapter3_Error001" → 1
        var trailingMatch = TrailingDigitsRegex.Match(name);
        if (trailingMatch.Success && int.TryParse(trailingMatch.Groups[1].Value, out var trailingNum))
            return trailingNum;

        return null;
    }

    #region Cache

    private static string ComputeCrxFingerprint(IReadOnlyList<CrxPickupTarget> targets)
    {
        var pairs = targets
            .OrderBy(t => t.ErrorNumber)
            .Select(t => $"{t.ErrorNumber}:{t.ChapterStem}:{t.SentenceId}:{t.ShouldBeText}");
        var joined = CrxFingerprintVersion + ";" + string.Join(";", pairs);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private string? GetCacheDir()
    {
        var workDir = _workspace.WorkingDirectory;
        if (string.IsNullOrEmpty(workDir))
            return null;

        var dir = Path.Combine(workDir, ".polish");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private PickupAssetCache? TryReadAssetCache()
    {
        var cacheDir = GetCacheDir();
        if (cacheDir == null) return null;

        var cachePath = Path.Combine(cacheDir, "pickup-assets-cache.json");
        if (!File.Exists(cachePath)) return null;

        try
        {
            var json = File.ReadAllText(cachePath);
            return JsonSerializer.Deserialize<PickupAssetCache>(json);
        }
        catch
        {
            return null;
        }
    }

    private void WriteAssetCache(PickupAssetCache cache)
    {
        var cacheDir = GetCacheDir();
        if (cacheDir == null) return;

        try
        {
            var cachePath = Path.Combine(cacheDir, "pickup-assets-cache.json");
            var json = JsonSerializer.Serialize(cache, CacheJsonOptions);
            File.WriteAllText(cachePath, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write pickup asset cache: {Message}", ex.Message);
        }
    }

    #endregion

    #region Helpers

    private static (IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched) SplitMatchedUnmatched(
        IReadOnlyList<PickupAsset> assets,
        double unmatchedThreshold)
    {
        var matched = new List<PickupAsset>();
        var unmatched = new List<PickupAsset>();

        foreach (var asset in assets)
        {
            if (asset.MatchedErrorNumber == null || asset.Confidence < unmatchedThreshold)
                unmatched.Add(asset);
            else
                matched.Add(asset);
        }

        return (matched, unmatched);
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

    #endregion
}
