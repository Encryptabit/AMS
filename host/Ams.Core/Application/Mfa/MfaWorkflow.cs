using System.IO;
using System.Text;
using Ams.Core.Application.Processes;
using Ams.Core.Application.Mfa.Models;

using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Processors;
using Ams.Core.Processors.Alignment.Mfa;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Application.Mfa;

public static class MfaWorkflow
{
    internal static async Task RunChapterAsync(
        ChapterContext chapterContext,
        FileInfo audioFile,
        FileInfo hydrateFile,
        string chapterStem,
        DirectoryInfo chapterDirectory,
        CancellationToken cancellationToken,
        bool useDedicatedProcess = false,
        string? workspaceRoot = null,
        MfaBeamSettings? beamSettings = null,
        bool disableChunkedMfa = false,
        bool requireAsrChunkAudio = false)
    {
        if (!useDedicatedProcess)
        {
            await MfaProcessSupervisor.EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!audioFile.Exists)
        {
            throw new FileNotFoundException("Audio file not found", audioFile.FullName);
        }

        if (!hydrateFile.Exists)
        {
            throw new FileNotFoundException("Hydrate JSON not found", hydrateFile.FullName);
        }

        var alignmentDir = EnsureDirectory(Path.Combine(chapterDirectory.FullName, "alignment"));
        var corpusDir = EnsureDirectory(Path.Combine(alignmentDir, "corpus"));
        var mfaCopyDir = EnsureDirectory(Path.Combine(alignmentDir, "mfa"));

        var mfaRoot = ResolveMfaRoot(workspaceRoot);
        CleanupMfaArtifacts(mfaRoot, chapterStem);

        var textGridCopyPath = Path.Combine(mfaCopyDir, chapterStem + ".TextGrid");
        if (File.Exists(textGridCopyPath))
        {
            try
            {
                File.Delete(textGridCopyPath);
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to delete existing TextGrid copy {Path}: {Message}", textGridCopyPath, ex.Message);
            }
        }

        // Determine whether to use chunked or single-utterance corpus
        var chunkPlan = chapterContext.Documents.ChunkPlan;
        var chunkAudio = chapterContext.Documents.ChunkAudio;
        var hydrate = chapterContext.Documents.HydratedTranscript;
        MfaChunkCorpusBuilder.ChunkCorpusResult? chunkCorpus = null;

        if (disableChunkedMfa && chunkPlan is not null && chunkPlan.Chunks.Count > 1)
        {
            Log.Debug("Chunked MFA disabled by rollout flag; using single-utterance corpus path");
        }
        else if (chunkPlan is not null && hydrate is not null && chunkPlan.Chunks.Count > 1)
        {
            // Clean the corpus directory for fresh chunk output
            CleanCorpusDirectory(corpusDir);

            var audioBuffer = AudioProcessor.Decode(audioFile.FullName);
            chunkCorpus = MfaChunkCorpusBuilder.Build(
                audioBuffer,
                chunkPlan,
                hydrate,
                corpusDir,
                chunkAudio,
                requireAsrChunkAudio,
                chapterContext.Documents.Asr);

            Log.Info("Using chunked MFA corpus: {Count} utterances from shared chunk plan", chunkCorpus.Utterances.Count);
        }
        else
        {
            // Legacy single-utterance path
            var stagedAudioPath = Path.Combine(corpusDir, chapterStem + ".wav");
            StageAudio(audioFile, stagedAudioPath);

            var corpusSource = new FileInfo(Path.Combine(chapterDirectory.FullName, chapterStem + ".asr.corpus.txt"));
            var labPath = Path.Combine(corpusDir, chapterStem + ".lab");
            await WriteLabFileAsync(hydrateFile, chapterContext, labPath, null, cancellationToken).ConfigureAwait(false);

            if (chunkPlan is not null && chunkPlan.Chunks.Count <= 1)
            {
                Log.Debug("Chunk plan has {Count} chunk(s); using single-utterance corpus path", chunkPlan.Chunks.Count);
            }
        }

        var dictionaryModel = MfaService.DefaultDictionaryModel;
        var acousticModel = MfaService.DefaultAcousticModel;
        var g2pModel = MfaService.DefaultG2pModel;

        var g2pOutputPath = Path.Combine(mfaRoot, chapterStem + ".g2p.txt");
        var customDictionaryPath = Path.Combine(mfaRoot, chapterStem + ".dictionary.zip");
        var alignOutputDir = Path.Combine(mfaRoot, chapterStem + ".align");

        var resolvedBeam = beamSettings ?? MfaBeamSettings.Resolve(MfaBeamProfile.Balanced);
        Log.Debug("MFA beam settings: beam={Beam}, retryBeam={RetryBeam}", resolvedBeam.Beam, resolvedBeam.RetryBeam);

        var baseContext = new MfaChapterContext
        {
            CorpusDirectory = corpusDir,
            OutputDirectory = alignOutputDir,
            WorkingDirectory = alignmentDir,
            DictionaryModel = dictionaryModel,
            AcousticModel = acousticModel,
            G2pModel = g2pModel,
            G2pOutputPath = g2pOutputPath,
            CustomDictionaryPath = customDictionaryPath,
            Beam = resolvedBeam.Beam,
            RetryBeam = resolvedBeam.RetryBeam,
            SingleSpeaker = true,
            CleanOutput = true
        };

        var service = new MfaService(useDedicatedProcess, useDedicatedProcess ? mfaRoot : null);

        Log.Debug("Running MFA validate on corpus {CorpusDir}", corpusDir);
        var oovListPath = FindOovListFile(mfaRoot);
        var sanitizedOovPath = oovListPath is not null
            ? CreateSanitizedOovList(mfaRoot, chapterStem, oovListPath)
            : null;

        var hasRealOovs = sanitizedOovPath is not null;

        bool customDictionaryAvailable = false;

        if (hasRealOovs)
        {
            Log.Debug("Generating pronunciations for OOV terms ({OovFile})", sanitizedOovPath);
            var g2pContext = baseContext with { OovListPath = sanitizedOovPath };
            var g2pResult = await service.GeneratePronunciationsAsync(g2pContext, cancellationToken)
                .ConfigureAwait(false);
            EnsureSuccess("mfa g2p", g2pResult);

            if (!File.Exists(g2pOutputPath) || new FileInfo(g2pOutputPath).Length == 0)
            {
                Log.Debug("G2P output missing or empty ({Path}); skipping custom dictionary stage", g2pOutputPath);
            }
            else
            {
                Log.Debug("Adding pronunciations to dictionary ({DictionaryOutput})", customDictionaryPath);
                var addWordsContext = baseContext with { OovListPath = sanitizedOovPath };
                var addWordsResult =
                    await service.AddWordsAsync(addWordsContext, cancellationToken).ConfigureAwait(false);
                EnsureSuccess("mfa model add_words", addWordsResult);

                customDictionaryAvailable = File.Exists(customDictionaryPath);
            }
        }
        else
        {
            Log.Debug("No substantive OOV entries detected; skipping G2P/add_words");
        }

        var alignContext = customDictionaryAvailable
            ? baseContext
            : baseContext with { CustomDictionaryPath = null };

        Log.Debug("Running MFA align for chapter {Chapter} ({Mode})", chapterStem,
            chunkCorpus is not null ? $"chunked, {chunkCorpus.Utterances.Count} utterances" : "single-utterance");

        if (chunkCorpus is not null)
        {
            // Chunked path: attempt chunked alignment, fall back to single-utterance on total failure.
            // Chunked alignment can fail when ASR timings are too rough for accurate chunk-to-text mapping
            // (chicken-and-egg: MFA produces accurate timings, but chunk labs depend on pre-MFA timings).
            try
            {
                var alignResult = await service.AlignAsync(alignContext, cancellationToken).ConfigureAwait(false);
                EnsureSuccess("mfa align", alignResult);
            }
            catch (InvalidOperationException ex)
            {
                Log.Warn(
                    "Chunked MFA alignment failed ({Message}); falling back to single-utterance corpus",
                    ex.Message);

                // Rebuild corpus as single utterance (same as legacy path)
                CleanCorpusDirectory(corpusDir);
                var stagedAudioPath = Path.Combine(corpusDir, chapterStem + ".wav");
                StageAudio(audioFile, stagedAudioPath);
                var labPath = Path.Combine(corpusDir, chapterStem + ".lab");
                await WriteLabFileAsync(hydrateFile, chapterContext, labPath, null, cancellationToken)
                    .ConfigureAwait(false);

                // Discard chunk corpus so downstream skips aggregation
                chunkCorpus = null;

                var fallbackResult = await service.AlignAsync(alignContext, cancellationToken).ConfigureAwait(false);
                EnsureSuccess("mfa align (single-utterance fallback)", fallbackResult);
            }
        }
        else
        {
            // Legacy single-utterance path with ASR corpus fallback
            var corpusSource = new FileInfo(Path.Combine(chapterDirectory.FullName, chapterStem + ".asr.corpus.txt"));
            var labPath = Path.Combine(corpusDir, chapterStem + ".lab");
            var alignUsingCorpus = false;

            while (true)
            {
                try
                {
                    var alignResult = await service.AlignAsync(alignContext, cancellationToken).ConfigureAwait(false);
                    EnsureSuccess("mfa align", alignResult);
                    break;
                }
                catch (InvalidOperationException ex) when (!alignUsingCorpus && corpusSource.Exists)
                {
                    alignUsingCorpus = true;
                    Log.Warn("MFA align failed using hydrate transcript ({Message}). Retrying with ASR corpus {Corpus}.",
                        ex.Message, corpusSource.FullName);
                    await WriteLabFileAsync(hydrateFile, chapterContext, labPath, corpusSource, cancellationToken)
                        .ConfigureAwait(false);
                    continue;
                }
            }
        }

        CopyIfExists(Path.Combine(mfaRoot, chapterStem + ".g2p.txt"),
            Path.Combine(mfaCopyDir, chapterStem + ".g2p.txt"));
        CopyIfExists(Path.Combine(mfaRoot, chapterStem + ".oov.cleaned.txt"),
            Path.Combine(mfaCopyDir, chapterStem + ".oov.cleaned.txt"));
        CopyIfExists(Path.Combine(mfaRoot, chapterStem + ".dictionary.zip"),
            Path.Combine(mfaCopyDir, chapterStem + ".dictionary.zip"));

        if (chunkCorpus is not null)
        {
            // Chunked path: collect per-utterance TextGrids and detect quality issues
            var chunkResults = CollectChunkTextGrids(chunkCorpus.Utterances, alignOutputDir, mfaCopyDir);

            // Adaptive strict retry: re-align only problematic chunks with strict beam
            var failedChunks = chunkResults
                .Where(r => r.Status != ChunkAlignmentStatus.Ok)
                .ToList();

            if (failedChunks.Count > 0 &&
                resolvedBeam.Beam < MfaBeamSettings.StrictRetry.Beam)
            {
                Log.Info(
                    "Adaptive retry: {Failed}/{Total} chunks need strict re-alignment " +
                    "(missing={Missing}, empty={Empty}, lowCoverage={LowCoverage})",
                    failedChunks.Count, chunkCorpus.Utterances.Count,
                    failedChunks.Count(r => r.Status == ChunkAlignmentStatus.MissingOutput),
                    failedChunks.Count(r => r.Status == ChunkAlignmentStatus.ParseFailure),
                    failedChunks.Count(r => r.Status == ChunkAlignmentStatus.LowCoverage));

                await RetryFailedChunksWithStrictBeamAsync(
                    failedChunks,
                    chunkCorpus,
                    service,
                    alignContext,
                    alignOutputDir,
                    mfaCopyDir,
                    cancellationToken).ConfigureAwait(false);
            }
            else if (failedChunks.Count > 0)
            {
                Log.Debug(
                    "Skipping adaptive retry: already at strict beam ({Beam}/{RetryBeam}), {Failed} chunks still problematic",
                    resolvedBeam.Beam, resolvedBeam.RetryBeam, failedChunks.Count);
            }

            // Aggregate per-chunk TextGrids into canonical chapter-level TextGrid
            var aggregatedCount = TextGridAggregationService.Aggregate(
                chunkCorpus.Utterances,
                mfaCopyDir,
                textGridCopyPath);

            if (aggregatedCount == 0)
            {
                Log.Warn("TextGrid aggregation produced no intervals; downstream merge may fail for {Chapter}", chapterStem);
            }
        }
        else
        {
            // Legacy single-utterance TextGrid collection
            var textGridCandidates = new[]
            {
                Path.Combine(alignOutputDir, "alignment", "mfa", chapterStem + ".TextGrid"),
                Path.Combine(alignOutputDir, chapterStem + ".TextGrid")
            };
            foreach (var candidate in textGridCandidates)
            {
                if (File.Exists(candidate))
                {
                    CopyIfExists(candidate, textGridCopyPath);
                    break;
                }
            }
        }

        CopyIfExists(Path.Combine(alignOutputDir, "alignment", "mfa", "alignment_analysis.csv"),
            Path.Combine(mfaCopyDir, "alignment_analysis.csv"));
    }

    private static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Removes all files from the corpus directory to prepare for fresh chunk output.
    /// </summary>
    private static void CleanCorpusDirectory(string corpusDir)
    {
        if (!Directory.Exists(corpusDir))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(corpusDir))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to clean corpus file {Path}: {Message}", file, ex.Message);
            }
        }
    }

    private static void StageAudio(FileInfo source, string destination)
    {
        var copyRequired = !File.Exists(destination);

        if (!copyRequired)
        {
            var srcInfo = source.LastWriteTimeUtc;
            var destInfo = File.GetLastWriteTimeUtc(destination);
            copyRequired = srcInfo > destInfo;
        }

        if (copyRequired)
        {
            File.Copy(source.FullName, destination, overwrite: true);
            File.SetLastWriteTimeUtc(destination, source.LastWriteTimeUtc);
        }
    }

    private static async Task WriteLabFileAsync(
        FileInfo hydrateFile,
        ChapterContext chapterContext,
        string labPath,
        FileInfo? corpusSource,
        CancellationToken cancellationToken)
    {
        if (corpusSource is { Exists: true })
        {
            var corpusLines = await File.ReadAllLinesAsync(corpusSource.FullName, cancellationToken)
                .ConfigureAwait(false);
            var normalizedCorpus = PrepareLabLines(corpusLines);
            if (normalizedCorpus.Count > 0)
            {
                Log.Debug("Using ASR corpus for MFA alignment ({Corpus})", corpusSource.FullName);
                await File.WriteAllTextAsync(labPath, string.Join(Environment.NewLine, normalizedCorpus), Encoding.UTF8,
                        cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            Log.Debug("ASR corpus at {Corpus} did not produce usable lines; falling back to hydrate",
                corpusSource.FullName);
        }

        var corpus = chapterContext.Documents.HydratedTranscript?.Sentences.Select(s => s.BookText).ToList() ?? [];

        if (corpus.Count != 0)
        {
            var normalized = PrepareLabLines(corpus);
            if (normalized.Count > 0)
            {
                await File.WriteAllTextAsync(labPath, string.Join(Environment.NewLine, normalized), Encoding.UTF8,
                    cancellationToken).ConfigureAwait(false);
                return;
            }
        }

        throw new InvalidOperationException(
            $"Unable to build MFA corpus lines for chapter {chapterContext.Descriptor.ChapterId}");
    }

    private static List<string> PrepareLabLines(IEnumerable<string> rawLines)
    {
        var prepared = new List<string>();
        foreach (var raw in rawLines)
        {
            var normalized = PrepareLabLine(raw);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                prepared.Add(normalized);
            }
        }

        return prepared;
    }

    private static string PrepareLabLine(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var parts = PronunciationHelper.ExtractPronunciationParts(text);
        if (parts.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(' ', parts);
    }

    private static bool EnsureSuccess(string stage, MfaCommandResult result, bool allowFailure = false)
    {
        foreach (var line in result.StdOut)
        {
            Log.Debug("{Stage}> {Line}", stage, line);
        }

        foreach (var line in result.StdErr)
        {
            Log.Debug("{Stage}! {Line}", stage, line);
        }

        Log.Debug("{Stage} command: {Command}", stage, result.Command);

        if (result.ExitCode != 0)
        {
            static string FormatLines(IEnumerable<string> lines, int limit)
            {
                var builder = new StringBuilder();
                int count = 0;
                foreach (var line in lines)
                {
                    if (count >= limit)
                    {
                        builder.AppendLine("... (truncated)");
                        break;
                    }

                    builder.AppendLine(line);
                    count++;
                }

                return builder.ToString();
            }

            if (!allowFailure)
            {
                var stdoutSnippet = FormatLines(result.StdOut, 20);
                var stderrSnippet = FormatLines(result.StdErr, 20);

                var message = new StringBuilder();
                message.AppendLine($"{stage} failed with exit code {result.ExitCode} (command: {result.Command})");
                if (stdoutSnippet.Length > 0)
                {
                    message.AppendLine("Stdout:");
                    message.Append(stdoutSnippet);
                }

                if (stderrSnippet.Length > 0)
                {
                    message.AppendLine("Stderr:");
                    message.Append(stderrSnippet);
                }

                throw new InvalidOperationException(message.ToString().TrimEnd());
            }

            Log.Debug("{Stage} returned exit code {ExitCode}; ignoring due to allowFailure", stage, result.ExitCode);
            return false;
        }

        return true;
    }

    private static bool IsZeroDivision(MfaCommandResult result)
    {
        return result.StdErr.Any(line => line.Contains("ZeroDivisionError", StringComparison.OrdinalIgnoreCase));
    }

    private static string? FindOovListFile(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "oovs_found*.txt", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to probe for OOV list: {Message}", ex.Message);
            return null;
        }
    }

    private static string? CreateSanitizedOovList(string mfaRoot, string chapterStem, string rawOovPath)
    {
        var cleanedPath = Path.Combine(mfaRoot, $"{chapterStem}.oov.cleaned.txt");

        try
        {
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in File.ReadLines(rawOovPath))
            {
                var token = raw.Replace("\ufeff", string.Empty)
                    .Trim()
                    .Trim('"', '\'', '`');

                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (!token.Any(char.IsLetter))
                {
                    continue;
                }

                unique.Add(token);
            }

            if (unique.Count == 0)
            {
                return null;
            }

            File.WriteAllLines(cleanedPath, unique.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
            return cleanedPath;
        }
        catch (Exception ex)
        {
            Log.Debug("Unable to sanitize OOV list {Path}: {Message}", rawOovPath, ex.Message);
            return null;
        }
    }

    private static void CleanupMfaArtifacts(string mfaRoot, string chapterStem)
    {
        if (!Directory.Exists(mfaRoot))
        {
            return;
        }

        static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to delete stale MFA artifact {Path}: {Message}", path, ex.Message);
            }
        }

        TryDelete(Path.Combine(mfaRoot, $"{chapterStem}.g2p.txt"));
        TryDelete(Path.Combine(mfaRoot, $"{chapterStem}.dictionary.zip"));
        TryDelete(Path.Combine(mfaRoot, $"{chapterStem}.oov.cleaned.txt"));

        TryDeleteDirectory(Path.Combine(mfaRoot, $"{chapterStem}.align"));
        TryDeleteDirectory(Path.Combine(mfaRoot, $"{chapterStem}.g2p"));
        TryDeleteDirectory(Path.Combine(mfaRoot, $"{chapterStem}.oov.cleaned"));
        // MFA itself uses the default workspace root (now Documents/MFA_1) for shared scratch data.
        // Parallel alignment jobs can hold open sqlite handles (corpus.db), so deleting
        // this directory per chapter causes needless contention/log noise.
        // Leave it intact so concurrent runs can share the workspace safely.

        TryDelete(Path.Combine(mfaRoot, "oov_counts_english_mfa.txt"));
        TryDelete(Path.Combine(mfaRoot, "utterance_oovs.txt"));
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to delete stale MFA directory {Path}: {Message}", path, ex.Message);
        }
    }

    private static string ResolveMfaRoot(string? overrideRoot = null)
    {
        return MfaWorkspaceResolver.ResolvePreferredRoot(overrideRoot);
    }

    private static void CopyIfExists(string sourcePath, string destinationPath)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                return;
            }

            var destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(sourcePath, destinationPath, overwrite: true);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to copy MFA artifact from {Source} to {Destination}: {Message}", sourcePath,
                destinationPath, ex.Message);
        }
    }

    // ----------------------------------------------------------------
    // Adaptive strict retry: chunk-level quality detection and retry
    // ----------------------------------------------------------------

    internal enum ChunkAlignmentStatus
    {
        Ok,
        MissingOutput,
        ParseFailure,
        LowCoverage
    }

    internal sealed record ChunkAlignmentResult(
        MfaChunkCorpusBuilder.UtteranceEntry Utterance,
        ChunkAlignmentStatus Status,
        int WordIntervalCount);

    /// <summary>
    /// Minimum ratio of word intervals to expected coverage (chunk duration / 0.3s per word estimate).
    /// Chunks below this threshold are considered low-coverage and eligible for strict retry.
    /// </summary>
    private const double MinCoverageRatio = 0.15;

    /// <summary>
    /// Collects per-chunk TextGrid outputs from the alignment directory into the copy directory,
    /// evaluating quality for each chunk.
    /// </summary>
    private static List<ChunkAlignmentResult> CollectChunkTextGrids(
        IReadOnlyList<MfaChunkCorpusBuilder.UtteranceEntry> utterances,
        string alignOutputDir,
        string mfaCopyDir)
    {
        var results = new List<ChunkAlignmentResult>(utterances.Count);

        foreach (var utt in utterances)
        {
            var destPath = Path.Combine(mfaCopyDir, utt.UtteranceName + ".TextGrid");
            var sourcePath = FindUtteranceTextGrid(alignOutputDir, utt.UtteranceName);

            if (sourcePath is null)
            {
                results.Add(new ChunkAlignmentResult(utt, ChunkAlignmentStatus.MissingOutput, 0));
                continue;
            }

            CopyIfExists(sourcePath, destPath);

            // Evaluate quality: parse word intervals
            int wordCount;
            try
            {
                var intervals = TextGridParser.ParseWordIntervals(destPath);
                wordCount = intervals.Count(iv =>
                    !string.IsNullOrWhiteSpace(iv.Text) &&
                    !string.Equals(iv.Text, "sp", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(iv.Text, "sil", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Log.Debug("TextGrid parse failure for chunk {Chunk}: {Message}", utt.UtteranceName, ex.Message);
                results.Add(new ChunkAlignmentResult(utt, ChunkAlignmentStatus.ParseFailure, 0));
                continue;
            }

            if (wordCount == 0)
            {
                results.Add(new ChunkAlignmentResult(utt, ChunkAlignmentStatus.ParseFailure, 0));
                continue;
            }

            // Low-coverage heuristic: estimate expected words from chunk duration
            var chunkDuration = utt.ChunkEndSec - utt.ChunkStartSec;
            var expectedWords = chunkDuration / 0.3; // ~3.3 words/sec average speech rate
            var coverageRatio = expectedWords > 0 ? wordCount / expectedWords : 1.0;

            if (coverageRatio < MinCoverageRatio)
            {
                Log.Debug(
                    "Low coverage for chunk {Chunk}: {Actual} words vs ~{Expected:F0} expected (ratio={Ratio:F2})",
                    utt.UtteranceName, wordCount, expectedWords, coverageRatio);
                results.Add(new ChunkAlignmentResult(utt, ChunkAlignmentStatus.LowCoverage, wordCount));
                continue;
            }

            results.Add(new ChunkAlignmentResult(utt, ChunkAlignmentStatus.Ok, wordCount));
        }

        return results;
    }

    /// <summary>
    /// Retries alignment for failed chunks using strict beam settings.
    /// Only the failed chunks are re-aligned; successful chunks are untouched.
    /// </summary>
    private static async Task RetryFailedChunksWithStrictBeamAsync(
        List<ChunkAlignmentResult> failedChunks,
        MfaChunkCorpusBuilder.ChunkCorpusResult chunkCorpus,
        MfaService service,
        MfaChapterContext alignContext,
        string alignOutputDir,
        string mfaCopyDir,
        CancellationToken cancellationToken)
    {
        var strictBeam = MfaBeamSettings.StrictRetry;

        // Build a subset corpus containing only the failed utterance wav+lab files.
        // This ensures MFA only re-aligns the problematic chunks, preserving throughput.
        var retryCorpusDir = Path.Combine(
            Path.GetDirectoryName(alignContext.CorpusDirectory)!,
            "retry-corpus");
        if (Directory.Exists(retryCorpusDir))
            Directory.Delete(retryCorpusDir, recursive: true);
        Directory.CreateDirectory(retryCorpusDir);

        foreach (var failed in failedChunks)
        {
            var uttName = failed.Utterance.UtteranceName;
            foreach (var ext in new[] { ".wav", ".lab" })
            {
                var src = Path.Combine(alignContext.CorpusDirectory, uttName + ext);
                if (File.Exists(src))
                    File.Copy(src, Path.Combine(retryCorpusDir, uttName + ext));
            }
        }

        var retryContext = alignContext with
        {
            Beam = strictBeam.Beam,
            RetryBeam = strictBeam.RetryBeam,
            CorpusDirectory = retryCorpusDir,
        };

        Log.Info(
            "Strict retry: beam={Beam}, retryBeam={RetryBeam} for {Count} chunks",
            strictBeam.Beam, strictBeam.RetryBeam, failedChunks.Count);

        try
        {
            var retryResult = await service.AlignAsync(retryContext, cancellationToken).ConfigureAwait(false);
            var retrySuccess = EnsureSuccess("mfa align (strict retry)", retryResult, allowFailure: true);

            if (!retrySuccess)
            {
                Log.Warn("Strict retry alignment failed; keeping initial results for all chunks");
                return;
            }

            // Re-collect TextGrids only for the originally failed chunks
            var recovered = 0;
            foreach (var failed in failedChunks)
            {
                var utt = failed.Utterance;
                var retrySrc = FindUtteranceTextGrid(alignOutputDir, utt.UtteranceName);
                if (retrySrc is null)
                {
                    Log.Debug("Strict retry: still no output for chunk {Chunk}", utt.UtteranceName);
                    continue;
                }

                var destPath = Path.Combine(mfaCopyDir, utt.UtteranceName + ".TextGrid");
                CopyIfExists(retrySrc, destPath);

                // Verify the retry actually improved things
                try
                {
                    var intervals = TextGridParser.ParseWordIntervals(destPath);
                    var retryWordCount = intervals.Count(iv =>
                        !string.IsNullOrWhiteSpace(iv.Text) &&
                        !string.Equals(iv.Text, "sp", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(iv.Text, "sil", StringComparison.OrdinalIgnoreCase));

                    if (retryWordCount > failed.WordIntervalCount)
                    {
                        recovered++;
                        Log.Debug(
                            "Strict retry improved chunk {Chunk}: {Before} -> {After} word intervals",
                            utt.UtteranceName, failed.WordIntervalCount, retryWordCount);
                    }
                    else
                    {
                        Log.Debug(
                            "Strict retry did not improve chunk {Chunk}: {Before} -> {After} word intervals",
                            utt.UtteranceName, failed.WordIntervalCount, retryWordCount);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug("Strict retry TextGrid parse failed for chunk {Chunk}: {Message}",
                        utt.UtteranceName, ex.Message);
                }
            }

            Log.Info(
                "Strict retry complete: {Recovered}/{Failed} chunks improved",
                recovered, failedChunks.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Warn("Strict retry alignment threw: {Message}; keeping initial results", ex.Message);
        }
        finally
        {
            // Clean up subset corpus directory
            try { if (Directory.Exists(retryCorpusDir)) Directory.Delete(retryCorpusDir, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    /// <summary>
    /// Finds the TextGrid output file for a given utterance name across known MFA output locations.
    /// </summary>
    private static string? FindUtteranceTextGrid(string alignOutputDir, string utteranceName)
    {
        var candidates = new[]
        {
            Path.Combine(alignOutputDir, "alignment", "mfa", utteranceName + ".TextGrid"),
            Path.Combine(alignOutputDir, utteranceName + ".TextGrid")
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
