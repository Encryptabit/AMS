using System.IO;
using System.Text;
using Ams.Core.Application.Processes;
using Ams.Core.Artifacts.Alignment.Mfa;
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
        string? workspaceRoot = null)
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

        var mfaRoot = ResolveMfaRoot(useDedicatedProcess ? workspaceRoot : null);
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

        var stagedAudioPath = Path.Combine(corpusDir, chapterStem + ".wav");
        StageAudio(audioFile, stagedAudioPath);

        var corpusSource = new FileInfo(Path.Combine(chapterDirectory.FullName, chapterStem + ".asr.corpus.txt"));
        var labPath = Path.Combine(corpusDir, chapterStem + ".lab");
        await WriteLabFileAsync(hydrateFile, chapterContext, labPath, null, cancellationToken).ConfigureAwait(false);

        var dictionaryModel = MfaService.DefaultDictionaryModel;
        var acousticModel = MfaService.DefaultAcousticModel;
        var g2pModel = MfaService.DefaultG2pModel;

        var g2pOutputPath = Path.Combine(mfaRoot, chapterStem + ".g2p.txt");
        var customDictionaryPath = Path.Combine(mfaRoot, chapterStem + ".dictionary.zip");
        var alignOutputDir = Path.Combine(mfaRoot, chapterStem + ".align");

        const int FastAlignBeam = 80;
        const int FastAlignRetryBeam = 200;

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
            Beam = FastAlignBeam,
            RetryBeam = FastAlignRetryBeam,
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
            var g2pResult = await service.GeneratePronunciationsAsync(g2pContext, cancellationToken).ConfigureAwait(false);
            EnsureSuccess("mfa g2p", g2pResult);

            if (!File.Exists(g2pOutputPath) || new FileInfo(g2pOutputPath).Length == 0)
            {
                Log.Debug("G2P output missing or empty ({Path}); skipping custom dictionary stage", g2pOutputPath);
            }
            else
            {
                Log.Debug("Adding pronunciations to dictionary ({DictionaryOutput})", customDictionaryPath);
                var addWordsContext = baseContext with { OovListPath = sanitizedOovPath };
                var addWordsResult = await service.AddWordsAsync(addWordsContext, cancellationToken).ConfigureAwait(false);
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

        Log.Debug("Running MFA align for chapter {Chapter}", chapterStem);
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
                Log.Warn("MFA align failed using hydrate transcript ({Message}). Retrying with ASR corpus {Corpus}.", ex.Message, corpusSource.FullName);
                await WriteLabFileAsync(hydrateFile, chapterContext, labPath, corpusSource, cancellationToken).ConfigureAwait(false);
                continue;
            }
        }

        CopyIfExists(Path.Combine(mfaRoot, chapterStem + ".g2p.txt"), Path.Combine(mfaCopyDir, chapterStem + ".g2p.txt"));
        CopyIfExists(Path.Combine(mfaRoot, chapterStem + ".oov.cleaned.txt"), Path.Combine(mfaCopyDir, chapterStem + ".oov.cleaned.txt"));
        CopyIfExists(Path.Combine(mfaRoot, chapterStem + ".dictionary.zip"), Path.Combine(mfaCopyDir, chapterStem + ".dictionary.zip"));

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

        CopyIfExists(Path.Combine(alignOutputDir, "alignment", "mfa", "alignment_analysis.csv"),
            Path.Combine(mfaCopyDir, "alignment_analysis.csv"));
    }

    private static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
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
            var corpusLines = await File.ReadAllLinesAsync(corpusSource.FullName, cancellationToken).ConfigureAwait(false);
            var normalizedCorpus = PrepareLabLines(corpusLines);
            if (normalizedCorpus.Count > 0)
            {
                Log.Debug("Using ASR corpus for MFA alignment ({Corpus})", corpusSource.FullName);
                await File.WriteAllTextAsync(labPath, string.Join(Environment.NewLine, normalizedCorpus), Encoding.UTF8, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            Log.Debug("ASR corpus at {Corpus} did not produce usable lines; falling back to hydrate", corpusSource.FullName);
        }

        var corpus = chapterContext.Documents.HydratedTranscript?.Sentences.Select(s => s.BookText).ToList() ?? [];

        if (corpus.Count != 0)
        {
            var normalized = PrepareLabLines(corpus);
            if (normalized.Count > 0)
            {
                await File.WriteAllTextAsync(labPath, string.Join(Environment.NewLine, normalized), Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                return;
            }
        }

        throw new InvalidOperationException($"Unable to build MFA corpus lines for chapter {chapterContext.Descriptor.ChapterId}");
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
        // MFA itself uses Documents/MFA/corpus as a shared working directory.
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
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            Directory.CreateDirectory(overrideRoot);
            return overrideRoot;
        }

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (string.IsNullOrWhiteSpace(documents))
        {
            throw new InvalidOperationException("Unable to resolve My Documents folder for MFA root.");
        }

        var mfaRoot = Path.Combine(documents, "MFA");
        Directory.CreateDirectory(mfaRoot);
        return mfaRoot;
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
            Log.Debug("Failed to copy MFA artifact from {Source} to {Destination}: {Message}", sourcePath, destinationPath, ex.Message);
        }
    }
}
