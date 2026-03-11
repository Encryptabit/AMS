using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ams.Core.Application.Mfa;
using Ams.Core.Application.Mfa.Models;
using Ams.Core.Application.Processes;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Asr;
using Ams.Core.Audio;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Processors.Alignment.Mfa;
using Ams.Core.Runtime.Book;
using Ams.Core.Services.Alignment;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Runs MFA forced alignment on the full pickup WAV and full ASR transcript,
/// then rewrites ASR token timings with MFA-accurate boundaries.
/// The refined ASR response is cached as a first-class artifact and reused.
/// </summary>
public class PickupMfaRefinementService
{
    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        WriteIndented = false
    };
    private static readonly JsonSerializerOptions ArtifactJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BlazorWorkspace _workspace;
    private readonly ChunkPlanningService _chunkPlanning = new();

    public PickupMfaRefinementService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
     /// Refines ASR token timings using full-file MFA forced alignment.
     /// Falls back to the original ASR response on any MFA failure.
     /// </summary>
    public async Task<AsrResponse> RefineAsrTimingsAsync(
        string pickupFilePath,
        AsrResponse asrResponse,
        CancellationToken ct)
    {
        var pickupBuffer = AudioProcessor.Decode(pickupFilePath);
        var asrReady = AsrAudioPreparer.PrepareForAsr(pickupBuffer);
        var chunkPlan = _chunkPlanning.GeneratePlan(asrReady, pickupFilePath);
        return await RefineAsrTimingsAsync(pickupFilePath, asrReady, chunkPlan, asrResponse, ct).ConfigureAwait(false);
    }

    public async Task<AsrResponse> RefineAsrTimingsAsync(
        string pickupFilePath,
        AudioBuffer asrReadyBuffer,
        ChunkPlanDocument chunkPlan,
        AsrResponse asrResponse,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupFilePath);
        ArgumentNullException.ThrowIfNull(asrReadyBuffer);
        ArgumentNullException.ThrowIfNull(chunkPlan);
        ArgumentNullException.ThrowIfNull(asrResponse);

        if (asrResponse.Tokens is not { Length: > 0 })
            return asrResponse;

        try
        {
            var workDir = _workspace.WorkingDirectory
                ?? throw new InvalidOperationException("No working directory set.");

            var (alignmentWords, wordToTokenIndex) = BuildAlignmentWords(asrResponse.Tokens);
            if (alignmentWords.Count == 0)
            {
                Log.Debug("MFA pickup refinement skipped: no usable ASR words for lab content");
                return asrResponse;
            }

            var artifactPaths = GetArtifactPaths(workDir);
            var artifactState = BuildArtifactState(pickupFilePath, alignmentWords, chunkPlan);
            var cachedState = TryReadArtifactState(artifactPaths.MetadataPath);
            var artifactsCurrent = IsArtifactStateCurrent(cachedState, artifactState);

            if (artifactsCurrent)
            {
                var cachedRefined = TryReadAsrResponseCache(artifactPaths.RefinedAsrPath);
                if (cachedRefined?.Tokens is { Length: > 0 })
                {
                    Log.Debug("MFA pickup ASR cache hit ({Path})", artifactPaths.RefinedAsrPath);
                    return cachedRefined;
                }
            }
            else
            {
                ResetArtifactPaths(artifactPaths);
            }

            EnsureArtifactDirectories(artifactPaths);
            WriteJsonArtifact(artifactPaths.ChunkPlanPath, chunkPlan);

            string? textGridPath;
            if (chunkPlan.Chunks.Count > 1)
            {
                var corpus = BuildChunkCorpus(
                    asrReadyBuffer,
                    chunkPlan,
                    asrResponse,
                    artifactPaths.ChunkAudioDir,
                    artifactPaths.CorpusDir);
                if (corpus.Utterances.Count == 0)
                {
                    Log.Warn("MFA pickup refinement skipped: chunked pickup corpus produced no utterances");
                    return asrResponse;
                }

                WriteJsonArtifact(
                    artifactPaths.ChunkAudioPath,
                    new ChunkAudioDocument(
                        Version: ChunkAudioDocument.CurrentVersion,
                        CreatedAtUtc: DateTime.UtcNow,
                        SourceAudioFingerprint: chunkPlan.SourceAudioFingerprint,
                        SampleRate: asrReadyBuffer.SampleRate,
                        Channels: asrReadyBuffer.Channels,
                        Chunks: corpus.ChunkAudioEntries));

                textGridPath = artifactPaths.TextGridPath;
                if (!File.Exists(textGridPath))
                {
                    await MfaProcessSupervisor.EnsureReadyAsync(ct).ConfigureAwait(false);

                    var pickupBeam = MfaBeamSettings.Resolve(MfaBeamProfile.Strict);
                    var context = new MfaChapterContext
                    {
                        CorpusDirectory = artifactPaths.CorpusDir,
                        OutputDirectory = artifactPaths.OutputDir,
                        WorkingDirectory = artifactPaths.RootDir,
                        DictionaryModel = MfaService.DefaultDictionaryModel,
                        AcousticModel = MfaService.DefaultAcousticModel,
                        G2pModel = MfaService.DefaultG2pModel,
                        Beam = pickupBeam.Beam,
                        RetryBeam = pickupBeam.RetryBeam,
                        SingleSpeaker = true,
                        CleanOutput = true
                    };

                    var service = new MfaService(useDedicatedProcess: false);
                    var alignResult = await service.AlignAsync(context, ct).ConfigureAwait(false);
                    LogMfaResult("mfa align (pickup)", alignResult);
                    if (alignResult.ExitCode != 0)
                    {
                        Log.Warn("Chunked MFA align failed for pickup (exit {Code}), using ASR timings", alignResult.ExitCode);
                        return asrResponse;
                    }

                    CollectChunkTextGrids(corpus.Utterances, artifactPaths.OutputDir, artifactPaths.MfaDir);
                    var aggregatedCount = AggregateChunkTextGrids(corpus.Utterances, artifactPaths.MfaDir, textGridPath);
                    if (aggregatedCount == 0)
                    {
                        Log.Warn("Chunked MFA pickup aggregation produced no intervals, using ASR timings");
                        return asrResponse;
                    }
                }
                else
                {
                    Log.Debug("MFA pickup chunked artifact reuse hit ({Path})", textGridPath);
                }
            }
            else
            {
                // Always stage the full pickup WAV (never clip).
                var stagedWavPath = Path.Combine(artifactPaths.CorpusDir, "pickup.wav");
                EnsureStagedPickupWav(pickupFilePath, stagedWavPath);

                var labPath = Path.Combine(artifactPaths.CorpusDir, "pickup.lab");
                var labContent = string.Join(' ', alignmentWords);
                if (string.IsNullOrWhiteSpace(labContent))
                {
                    Log.Debug("MFA pickup refinement skipped: lab content was empty");
                    return asrResponse;
                }

                await EnsureLabContentAsync(labPath, labContent, ct).ConfigureAwait(false);
                textGridPath = File.Exists(artifactPaths.TextGridPath)
                    ? artifactPaths.TextGridPath
                    : FindTextGridFile(artifactPaths.OutputDir);
            }

            if (textGridPath == null)
            {
                await MfaProcessSupervisor.EnsureReadyAsync(ct).ConfigureAwait(false);

                // Pickup refinement uses strict profile for maximum precision
                // on short single-utterance audio fragments.
                var pickupBeam = MfaBeamSettings.Resolve(MfaBeamProfile.Strict);

                var context = new MfaChapterContext
                {
                    CorpusDirectory = artifactPaths.CorpusDir,
                    OutputDirectory = artifactPaths.OutputDir,
                    WorkingDirectory = artifactPaths.RootDir,
                    DictionaryModel = MfaService.DefaultDictionaryModel,
                    AcousticModel = MfaService.DefaultAcousticModel,
                    G2pModel = MfaService.DefaultG2pModel,
                    Beam = pickupBeam.Beam,
                    RetryBeam = pickupBeam.RetryBeam,
                    SingleSpeaker = true,
                    CleanOutput = true
                };

                var service = new MfaService(useDedicatedProcess: false);

                var alignResult = await service.AlignAsync(context, ct).ConfigureAwait(false);
                LogMfaResult("mfa align (pickup)", alignResult);
                if (alignResult.ExitCode != 0)
                {
                    Log.Warn("MFA align failed for pickup (exit {Code}), using ASR timings", alignResult.ExitCode);
                    return asrResponse;
                }

                textGridPath = FindTextGridFile(artifactPaths.OutputDir);
            }
            else
            {
                Log.Debug("MFA pickup artifact reuse hit ({Path})", textGridPath);
            }

            if (textGridPath == null)
            {
                Log.Warn("MFA TextGrid not found after alignment, using ASR timings");
                return asrResponse;
            }

            if (!string.Equals(textGridPath, artifactPaths.TextGridPath, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(textGridPath, artifactPaths.TextGridPath, overwrite: true);
                textGridPath = artifactPaths.TextGridPath;
            }

            var intervals = TextGridParser.ParseWordIntervals(textGridPath);
            var tokenTimings = AlignMfaWordsToAsrTokens(intervals, alignmentWords, wordToTokenIndex);

            if (tokenTimings.Count == 0)
            {
                Log.Debug("MFA pickup refinement produced no token timing updates");
                return asrResponse;
            }

            var refinedTokens = ApplyRefinedTimings(asrResponse.Tokens, tokenTimings);
            var refinedResponse = asrResponse with { Tokens = refinedTokens };

            WriteAsrResponseCache(artifactPaths.RefinedAsrPath, refinedResponse);
            WriteArtifactState(artifactPaths.MetadataPath, artifactState);

            Log.Debug(
                "MFA pickup ASR refinement complete: {Updated}/{Total} token timings updated",
                tokenTimings.Count,
                asrResponse.Tokens.Length);

            return refinedResponse;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Warn("MFA pickup ASR refinement failed ({Message}), using ASR timings", ex.Message);
            return asrResponse;
        }
    }

    #region Alignment

    private static (List<string> Words, List<int> WordToTokenIndex) BuildAlignmentWords(
        IReadOnlyList<AsrToken> tokens)
    {
        var words = new List<string>();
        var wordToTokenIndex = new List<int>();

        for (var tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            var token = tokens[tokenIndex];
            var parts = PronunciationHelper.ExtractPronunciationParts(token.Word);

            if (parts.Count == 0)
            {
                var fallback = NormalizeAlignmentWord(token.Word);
                if (fallback.Length > 0)
                {
                    words.Add(fallback);
                    wordToTokenIndex.Add(tokenIndex);
                }

                continue;
            }

            foreach (var part in parts)
            {
                var normalized = NormalizeAlignmentWord(part);
                if (normalized.Length == 0)
                    continue;

                words.Add(normalized);
                wordToTokenIndex.Add(tokenIndex);
            }
        }

        return (words, wordToTokenIndex);
    }

    private static string NormalizeAlignmentWord(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch) || ch == '\'')
                sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString();
    }

    private static Dictionary<int, (double Start, double End)> AlignMfaWordsToAsrTokens(
        IReadOnlyList<TextGridInterval> intervals,
        IReadOnlyList<string> alignmentWords,
        IReadOnlyList<int> wordToTokenIndex)
    {
        if (alignmentWords.Count == 0 || intervals.Count == 0)
            return new Dictionary<int, (double Start, double End)>();

        var textGridWords = intervals
            .Where(iv => !string.IsNullOrWhiteSpace(iv.Text)
                         && !string.Equals(iv.Text, "sp", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(iv.Text, "sil", StringComparison.OrdinalIgnoreCase))
            .Select(iv => new TextGridWord(iv.Text, iv.Start, iv.End))
            .ToArray();

        if (textGridWords.Length == 0)
            return new Dictionary<int, (double Start, double End)>();

        var tokenTimings = new Dictionary<int, (double Start, double End)>();
        var wordTargets = new List<WordTarget>(alignmentWords.Count);

        for (var alignmentIndex = 0; alignmentIndex < alignmentWords.Count; alignmentIndex++)
        {
            var localAlignmentIndex = alignmentIndex;
            var tokenIndex = wordToTokenIndex[alignmentIndex];

            wordTargets.Add(new WordTarget(localAlignmentIndex, (start, end, _) =>
            {
                if (end <= start)
                    return;

                if (tokenTimings.TryGetValue(tokenIndex, out var existing))
                {
                    tokenTimings[tokenIndex] = (Math.Min(existing.Start, start), Math.Max(existing.End, end));
                }
                else
                {
                    tokenTimings[tokenIndex] = (start, end);
                }
            }));
        }

        string GetBookToken(int index)
            => index >= 0 && index < alignmentWords.Count ? alignmentWords[index] : string.Empty;

        MfaTimingMerger.MergeAndApply(
            textGridWords,
            GetBookToken,
            0,
            alignmentWords.Count - 1,
            wordTargets,
            Array.Empty<SentenceTarget>(),
            debugLog: msg => Log.Debug("MFA pickup merge: {Message}", msg));

        return tokenTimings;
    }

    private static AsrToken[] ApplyRefinedTimings(
        IReadOnlyList<AsrToken> originalTokens,
        IReadOnlyDictionary<int, (double Start, double End)> refinedTimings)
    {
        var refined = new AsrToken[originalTokens.Count];

        for (var i = 0; i < originalTokens.Count; i++)
        {
            var original = originalTokens[i];
            if (refinedTimings.TryGetValue(i, out var timing) && IsPlausibleTokenTiming(original, timing))
            {
                refined[i] = new AsrToken(
                    StartTime: timing.Start,
                    Duration: Math.Max(0.01, timing.End - timing.Start),
                    Word: original.Word);
            }
            else
            {
                refined[i] = original;
            }
        }

        return refined;
    }

    private static bool IsPlausibleTokenTiming(AsrToken original, (double Start, double End) candidate)
    {
        if (double.IsNaN(candidate.Start) || double.IsNaN(candidate.End))
            return false;
        if (candidate.End <= candidate.Start)
            return false;

        var refinedDuration = candidate.End - candidate.Start;
        if (refinedDuration < 0.01 || refinedDuration > 3.0)
            return false;

        if (original.Duration > 0.001)
        {
            var durationRatio = refinedDuration / original.Duration;
            if (durationRatio < 0.15 || durationRatio > 8.0)
                return false;
        }

        return true;
    }

    private static PickupChunkCorpus BuildChunkCorpus(
        AudioBuffer asrReadyBuffer,
        ChunkPlanDocument chunkPlan,
        AsrResponse asrResponse,
        string chunkAudioDir,
        string corpusDir)
    {
        CleanDirectory(chunkAudioDir, "*.wav");
        CleanDirectory(corpusDir, "*.wav", "*.lab");

        var utterances = new List<PickupChunkUtterance>(chunkPlan.Chunks.Count);
        var chunkAudioEntries = new List<ChunkAudioEntry>(chunkPlan.Chunks.Count);
        for (var i = 0; i < chunkPlan.Chunks.Count; i++)
        {
            var chunk = chunkPlan.Chunks[i];
            var utteranceName = $"utt-{i:D4}";
            var labWords = ExtractChunkAlignmentWords(
                asrResponse.Tokens,
                chunk.StartSec,
                chunk.EndSec,
                isLastChunk: i == chunkPlan.Chunks.Count - 1);

            if (labWords.Count == 0)
            {
                Log.Debug(
                    "Pickup MFA chunk {ChunkId} ({Utterance}) produced no alignment words; skipping",
                    chunk.ChunkId,
                    utteranceName);
                continue;
            }

            var slice = asrReadyBuffer.Slice(chunk.StartSample, chunk.LengthSamples);
            if (slice.Length <= 0)
            {
                Log.Debug(
                    "Pickup MFA chunk {ChunkId} ({Utterance}) produced no audio; skipping",
                    chunk.ChunkId,
                    utteranceName);
                continue;
            }

            var chunkAudioPath = Path.Combine(chunkAudioDir, utteranceName + ".wav");
            var corpusAudioPath = Path.Combine(corpusDir, utteranceName + ".wav");
            var labPath = Path.Combine(corpusDir, utteranceName + ".lab");

            AudioProcessor.EncodeWav(chunkAudioPath, slice);
            File.Copy(chunkAudioPath, corpusAudioPath, overwrite: true);
            File.WriteAllText(labPath, string.Join(' ', labWords), Encoding.UTF8);

            chunkAudioEntries.Add(new ChunkAudioEntry(
                ChunkId: chunk.ChunkId,
                UtteranceName: utteranceName,
                StartSec: chunk.StartSec,
                EndSec: chunk.EndSec,
                WavPath: chunkAudioPath));

            utterances.Add(new PickupChunkUtterance(
                chunk.ChunkId,
                utteranceName,
                chunk.StartSec,
                chunk.EndSec));
        }

        Log.Debug(
            "Pickup MFA using shared chunk plan: {Utterances}/{Chunks} utterances emitted",
            utterances.Count,
            chunkPlan.Chunks.Count);

        return new PickupChunkCorpus(utterances, chunkAudioEntries);
    }

    private static List<string> ExtractChunkAlignmentWords(
        IReadOnlyList<AsrToken> tokens,
        double chunkStartSec,
        double chunkEndSec,
        bool isLastChunk)
    {
        var words = new List<string>();

        foreach (var token in tokens)
        {
            var midpoint = token.StartTime + (Math.Max(0.01, token.Duration) / 2.0);
            var inChunk = midpoint >= chunkStartSec && (midpoint < chunkEndSec || (isLastChunk && midpoint <= chunkEndSec));
            if (!inChunk)
                continue;

            var parts = PronunciationHelper.ExtractPronunciationParts(token.Word);
            if (parts.Count == 0)
            {
                var normalized = NormalizeAlignmentWord(token.Word);
                if (normalized.Length > 0)
                    words.Add(normalized);
                continue;
            }

            foreach (var part in parts)
            {
                var normalized = NormalizeAlignmentWord(part);
                if (normalized.Length > 0)
                    words.Add(normalized);
            }
        }

        return words;
    }

    private static void CollectChunkTextGrids(
        IReadOnlyList<PickupChunkUtterance> utterances,
        string alignOutputDir,
        string mfaCopyDir)
    {
        Directory.CreateDirectory(mfaCopyDir);
        foreach (var utterance in utterances)
        {
            var candidate = FindChunkTextGridFile(alignOutputDir, utterance.UtteranceName);
            if (candidate == null)
                continue;

            File.Copy(candidate, Path.Combine(mfaCopyDir, utterance.UtteranceName + ".TextGrid"), overwrite: true);
        }
    }

    private static string? FindChunkTextGridFile(string outputDir, string utteranceName)
    {
        var candidates = new[]
        {
            Path.Combine(outputDir, "alignment", "mfa", utteranceName + ".TextGrid"),
            Path.Combine(outputDir, utteranceName + ".TextGrid")
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static int AggregateChunkTextGrids(
        IReadOnlyList<PickupChunkUtterance> utterances,
        string chunkTextGridDirectory,
        string outputPath)
    {
        var allWordIntervals = new List<TextGridInterval>();
        var allPhoneIntervals = new List<TextGridInterval>();

        foreach (var utterance in utterances)
        {
            var textGridPath = Path.Combine(chunkTextGridDirectory, utterance.UtteranceName + ".TextGrid");
            if (!File.Exists(textGridPath))
                continue;

            foreach (var interval in TextGridParser.ParseWordIntervals(textGridPath))
            {
                allWordIntervals.Add(new TextGridInterval(
                    interval.Start + utterance.ChunkStartSec,
                    interval.End + utterance.ChunkStartSec,
                    interval.Text));
            }

            foreach (var interval in TextGridParser.ParsePhoneIntervals(textGridPath))
            {
                allPhoneIntervals.Add(new TextGridInterval(
                    interval.Start + utterance.ChunkStartSec,
                    interval.End + utterance.ChunkStartSec,
                    interval.Text));
            }
        }

        if (allWordIntervals.Count == 0)
            return 0;

        allWordIntervals.Sort((a, b) => a.Start.CompareTo(b.Start));
        allPhoneIntervals.Sort((a, b) => a.Start.CompareTo(b.Start));

        var xmin = Math.Min(
            allWordIntervals.Count > 0 ? allWordIntervals[0].Start : 0.0,
            allPhoneIntervals.Count > 0 ? allPhoneIntervals[0].Start : double.MaxValue);
        var xmax = Math.Max(
            allWordIntervals.Count > 0 ? allWordIntervals[^1].End : 0.0,
            allPhoneIntervals.Count > 0 ? allPhoneIntervals[^1].End : 0.0);

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        WriteTextGrid(outputPath, xmin, xmax, allWordIntervals, allPhoneIntervals);
        return allWordIntervals.Count;
    }

    private static void WriteTextGrid(
        string outputPath,
        double xmin,
        double xmax,
        IReadOnlyList<TextGridInterval> wordIntervals,
        IReadOnlyList<TextGridInterval> phoneIntervals)
    {
        var sb = new StringBuilder();
        var ci = CultureInfo.InvariantCulture;
        var tierCount = phoneIntervals.Count > 0 ? 2 : 1;

        sb.AppendLine("File type = \"ooTextFile\"");
        sb.AppendLine("Object class = \"TextGrid\"");
        sb.AppendLine();
        sb.AppendLine(string.Create(ci, $"xmin = {xmin}"));
        sb.AppendLine(string.Create(ci, $"xmax = {xmax}"));
        sb.AppendLine("tiers? <exists>");
        sb.AppendLine(string.Create(ci, $"size = {tierCount}"));
        sb.AppendLine("item []:");

        WriteTier(sb, ci, 1, "words", xmin, xmax, wordIntervals);
        if (phoneIntervals.Count > 0)
            WriteTier(sb, ci, 2, "phones", xmin, xmax, phoneIntervals);

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static void WriteTier(
        StringBuilder sb,
        CultureInfo ci,
        int tierIndex,
        string tierName,
        double xmin,
        double xmax,
        IReadOnlyList<TextGridInterval> intervals)
    {
        sb.AppendLine(string.Create(ci, $"    item [{tierIndex}]:"));
        sb.AppendLine("        class = \"IntervalTier\"");
        sb.AppendLine(string.Create(ci, $"        name = \"{tierName}\""));
        sb.AppendLine(string.Create(ci, $"        xmin = {xmin}"));
        sb.AppendLine(string.Create(ci, $"        xmax = {xmax}"));
        sb.AppendLine(string.Create(ci, $"        intervals: size = {intervals.Count}"));

        for (var i = 0; i < intervals.Count; i++)
        {
            var interval = intervals[i];
            sb.AppendLine(string.Create(ci, $"        intervals [{i + 1}]:"));
            sb.AppendLine(string.Create(ci, $"            xmin = {interval.Start}"));
            sb.AppendLine(string.Create(ci, $"            xmax = {interval.End}"));
            sb.AppendLine($"            text = \"{interval.Text.Replace("\"", "\"\"")}\"");
        }
    }

    private static void CleanDirectory(string directory, params string[] patterns)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var pattern in patterns)
        {
            foreach (var file in Directory.EnumerateFiles(directory, pattern))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Log.Debug("Failed to remove stale pickup MFA artifact {Path}: {Message}", file, ex.Message);
                }
            }
        }
    }

    private sealed record PickupChunkUtterance(
        int ChunkId,
        string UtteranceName,
        double ChunkStartSec,
        double ChunkEndSec);

    private sealed record PickupChunkCorpus(
        IReadOnlyList<PickupChunkUtterance> Utterances,
        IReadOnlyList<ChunkAudioEntry> ChunkAudioEntries);

    #endregion

    #region Cache / Artifacts

    private static PickupArtifactPaths GetArtifactPaths(string workDir)
    {
        var rootDir = Path.Combine(workDir, ".polish", "pickups");
        var alignmentDir = Path.Combine(rootDir, "alignment");
        var chunkAudioDir = Path.Combine(alignmentDir, "chunk-audio");
        var corpusDir = Path.Combine(alignmentDir, "corpus");
        var mfaDir = Path.Combine(alignmentDir, "mfa");
        var outputDir = Path.Combine(alignmentDir, "output");

        return new PickupArtifactPaths(
            RootDir: rootDir,
            AlignmentDir: alignmentDir,
            ChunkAudioDir: chunkAudioDir,
            CorpusDir: corpusDir,
            MfaDir: mfaDir,
            OutputDir: outputDir,
            ChunkPlanPath: Path.Combine(rootDir, "pickups.align.chunks.json"),
            ChunkAudioPath: Path.Combine(rootDir, "pickups.align.chunk-audio.json"),
            RefinedAsrPath: Path.Combine(rootDir, "pickups.asr.mfa.json"),
            MetadataPath: Path.Combine(rootDir, "pickups.meta.json"),
            TextGridPath: Path.Combine(mfaDir, "pickups.TextGrid"));
    }

    private static PickupArtifactState BuildArtifactState(
        string pickupFilePath,
        IReadOnlyList<string> alignmentWords,
        ChunkPlanDocument chunkPlan)
    {
        var fi = new FileInfo(pickupFilePath);
        return new PickupArtifactState(
            PickupFilePath: fi.FullName,
            PickupFileSizeBytes: fi.Length,
            PickupFileModifiedUtc: fi.LastWriteTimeUtc,
            AlignmentWordsHash: ComputeAlignmentWordsHash(alignmentWords),
            ChunkPlanHash: ComputeChunkPlanHash(chunkPlan));
    }

    private static string ComputeAlignmentWordsHash(IReadOnlyList<string> alignmentWords)
    {
        var joined = string.Join('|', alignmentWords);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private static string ComputeChunkPlanHash(ChunkPlanDocument chunkPlan)
    {
        var sb = new StringBuilder();
        sb.Append(chunkPlan.SourceAudioPath).Append('|')
            .Append(chunkPlan.SourceAudioFingerprint).Append('|')
            .Append(chunkPlan.Policy.SilenceThresholdDb).Append('|')
            .Append(chunkPlan.Policy.MinSilenceDurationMs).Append('|')
            .Append(chunkPlan.Policy.MinChunkDurationSec).Append('|')
            .Append(chunkPlan.Policy.MaxChunkDurationSec).Append('|')
            .Append(chunkPlan.Policy.SampleRate);

        foreach (var chunk in chunkPlan.Chunks)
        {
            sb.Append('|')
                .Append(chunk.ChunkId).Append(':')
                .Append(chunk.StartSample).Append(':')
                .Append(chunk.LengthSamples).Append(':')
                .Append(chunk.StartSec).Append(':')
                .Append(chunk.EndSec);
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private static PickupArtifactState? TryReadArtifactState(string metadataPath)
    {
        if (!File.Exists(metadataPath))
            return null;

        try
        {
            var json = File.ReadAllText(metadataPath);
            return JsonSerializer.Deserialize<PickupArtifactState>(json);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsArtifactStateCurrent(PickupArtifactState? existing, PickupArtifactState current)
    {
        return existing != null &&
               string.Equals(existing.PickupFilePath, current.PickupFilePath, StringComparison.OrdinalIgnoreCase) &&
               existing.PickupFileSizeBytes == current.PickupFileSizeBytes &&
               existing.PickupFileModifiedUtc == current.PickupFileModifiedUtc &&
               string.Equals(existing.AlignmentWordsHash, current.AlignmentWordsHash, StringComparison.Ordinal) &&
               string.Equals(existing.ChunkPlanHash, current.ChunkPlanHash, StringComparison.Ordinal);
    }

    private static void WriteArtifactState(string metadataPath, PickupArtifactState state)
        => WriteJsonArtifact(metadataPath, state);

    private static void EnsureArtifactDirectories(PickupArtifactPaths paths)
    {
        Directory.CreateDirectory(paths.RootDir);
        Directory.CreateDirectory(paths.AlignmentDir);
        Directory.CreateDirectory(paths.ChunkAudioDir);
        Directory.CreateDirectory(paths.CorpusDir);
        Directory.CreateDirectory(paths.MfaDir);
        Directory.CreateDirectory(paths.OutputDir);
    }

    private static void ResetArtifactPaths(PickupArtifactPaths paths)
    {
        TryDeleteFile(paths.ChunkPlanPath);
        TryDeleteFile(paths.ChunkAudioPath);
        TryDeleteFile(paths.RefinedAsrPath);
        TryDeleteFile(paths.MetadataPath);
        TryDeleteFile(paths.TextGridPath);

        CleanDirectory(paths.ChunkAudioDir, "*.wav");
        CleanDirectory(paths.CorpusDir, "*.wav", "*.lab");
        CleanDirectory(paths.MfaDir, "*.TextGrid");
        CleanDirectory(paths.OutputDir, "*.TextGrid", "*.csv", "*.json");
    }

    private static void TryDeleteFile(string path)
    {
        if (!File.Exists(path))
            return;

        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to remove stale pickup artifact {Path}: {Message}", path, ex.Message);
        }
    }

    private static AsrResponse? TryReadAsrResponseCache(string cachePath)
    {
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

    private static void WriteAsrResponseCache(string cachePath, AsrResponse response)
    {
        try
        {
            var dir = Path.GetDirectoryName(cachePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(response, CacheJsonOptions);
            File.WriteAllText(cachePath, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write MFA pickup ASR cache: {Message}", ex.Message);
        }
    }

    private static void WriteJsonArtifact<T>(string path, T value)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(value, ArtifactJsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write pickup artifact {Path}: {Message}", path, ex.Message);
        }
    }

    private sealed record PickupArtifactPaths(
        string RootDir,
        string AlignmentDir,
        string ChunkAudioDir,
        string CorpusDir,
        string MfaDir,
        string OutputDir,
        string ChunkPlanPath,
        string ChunkAudioPath,
        string RefinedAsrPath,
        string MetadataPath,
        string TextGridPath);

    private sealed record PickupArtifactState(
        string PickupFilePath,
        long PickupFileSizeBytes,
        DateTime PickupFileModifiedUtc,
        string AlignmentWordsHash,
        string ChunkPlanHash);

    #endregion

    #region MFA Helpers

    private static string? FindTextGridFile(string outputDir)
    {
        var candidates = new[]
        {
            Path.Combine(outputDir, "alignment", "mfa", "pickup.TextGrid"),
            Path.Combine(outputDir, "pickup.TextGrid")
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string? FindOovListFile(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "oovs_found*.txt", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static void LogMfaResult(string stage, MfaCommandResult result)
    {
        foreach (var line in result.StdOut)
            Log.Debug("{Stage}> {Line}", stage, line);
        foreach (var line in result.StdErr)
            Log.Debug("{Stage}! {Line}", stage, line);
        Log.Debug("{Stage} exit code: {ExitCode}", stage, result.ExitCode);
    }

    private static void EnsureStagedPickupWav(string sourcePath, string destinationPath)
    {
        var srcInfo = new FileInfo(sourcePath);
        var dstInfo = new FileInfo(destinationPath);

        if (!dstInfo.Exists || dstInfo.Length != srcInfo.Length || dstInfo.LastWriteTimeUtc != srcInfo.LastWriteTimeUtc)
        {
            File.Copy(sourcePath, destinationPath, overwrite: true);
            File.SetLastWriteTimeUtc(destinationPath, srcInfo.LastWriteTimeUtc);
        }
    }

    private static async Task EnsureLabContentAsync(string labPath, string content, CancellationToken ct)
    {
        if (File.Exists(labPath))
        {
            var existing = await File.ReadAllTextAsync(labPath, ct).ConfigureAwait(false);
            if (string.Equals(existing, content, StringComparison.Ordinal))
                return;
        }

        await File.WriteAllTextAsync(labPath, content, Encoding.UTF8, ct).ConfigureAwait(false);
    }

    #endregion
}
