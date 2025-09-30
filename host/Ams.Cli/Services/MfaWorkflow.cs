using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Alignment.Mfa;
using Ams.Core.Common;

namespace Ams.Cli.Services;

internal static class MfaWorkflow
{
    internal static async Task RunChapterAsync(
        FileInfo audioFile,
        FileInfo hydrateFile,
        string chapterStem,
        DirectoryInfo chapterDirectory,
        CancellationToken cancellationToken)
    {
        await MfaProcessSupervisor.EnsureReadyAsync(cancellationToken).ConfigureAwait(false);

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
        var mfaDir = EnsureDirectory(Path.Combine(alignmentDir, "mfa"));

        CleanupMfaArtifacts(mfaDir, chapterStem);

        var stagedAudioPath = Path.Combine(corpusDir, chapterStem + ".wav");
        StageAudio(audioFile, stagedAudioPath);

        var labPath = Path.Combine(corpusDir, chapterStem + ".lab");
        await WriteLabFileAsync(hydrateFile, labPath, cancellationToken).ConfigureAwait(false);

        var dictionaryModel = MfaService.DefaultDictionaryModel;
        var acousticModel = MfaService.DefaultAcousticModel;
        var g2pModel = MfaService.DefaultG2pModel;

        var g2pOutputPath = Path.Combine(mfaDir, chapterStem + ".g2p.txt");
        var customDictionaryPath = Path.Combine(mfaDir, chapterStem + ".dictionary.zip");

        var baseContext = new MfaChapterContext
        {
            CorpusDirectory = corpusDir,
            OutputDirectory = mfaDir,
            WorkingDirectory = alignmentDir,
            DictionaryModel = dictionaryModel,
            AcousticModel = acousticModel,
            G2pModel = g2pModel,
            G2pOutputPath = g2pOutputPath,
            CustomDictionaryPath = customDictionaryPath,
            Beam = 120,
            RetryBeam = 400,
            SingleSpeaker = true,
            CleanOutput = true
        };

        var service = MfaService.Default;

        try
        {
            Log.Info("Running MFA validate on corpus {CorpusDir}", corpusDir);
            var validateResult = await service.ValidateAsync(baseContext, cancellationToken).ConfigureAwait(false);
            var validateSucceeded = EnsureSuccess("mfa validate", validateResult, allowFailure: true);

            if (!validateSucceeded)
            {
                if (IsZeroDivision(validateResult))
                {
                    Log.Warn("mfa validate reported a ZeroDivisionError (likely due to very small corpora); continuing with generated artifacts");
                }
                else
                {
                    throw new InvalidOperationException("mfa validate failed");
                }
            }

            var oovListPath = FindOovListFile(mfaDir);
            var sanitizedOovPath = oovListPath is not null
                ? CreateSanitizedOovList(mfaDir, chapterStem, oovListPath)
                : null;

            var hasRealOovs = sanitizedOovPath is not null;

            bool customDictionaryAvailable = false;

            if (hasRealOovs)
            {
                Log.Info("Generating pronunciations for OOV terms ({OovFile})", sanitizedOovPath);
                var g2pContext = baseContext with { OovListPath = sanitizedOovPath };
                var g2pResult = await service.GeneratePronunciationsAsync(g2pContext, cancellationToken).ConfigureAwait(false);
                EnsureSuccess("mfa g2p", g2pResult);

                if (!File.Exists(g2pOutputPath) || new FileInfo(g2pOutputPath).Length == 0)
                {
                    Log.Warn("G2P output missing or empty ({Path}); skipping custom dictionary stage", g2pOutputPath);
                }
                else
                {
                    Log.Info("Adding pronunciations to dictionary ({DictionaryOutput})", customDictionaryPath);
                    var addWordsContext = baseContext with { OovListPath = sanitizedOovPath };
                    var addWordsResult = await service.AddWordsAsync(addWordsContext, cancellationToken).ConfigureAwait(false);
                    EnsureSuccess("mfa model add_words", addWordsResult);

                    customDictionaryAvailable = File.Exists(customDictionaryPath);
                }
            }
            else
            {
                Log.Info("No substantive OOV entries detected; skipping G2P/add_words");
            }

            var alignContext = customDictionaryAvailable
                ? baseContext
                : baseContext with { CustomDictionaryPath = null };

            Log.Info("Running MFA align for chapter {Chapter}", chapterStem);
            var alignResult = await service.AlignAsync(alignContext, cancellationToken).ConfigureAwait(false);
            EnsureSuccess("mfa align", alignResult);
        }
        finally
        {
            MfaProcessSupervisor.Shutdown();
        }
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

    private static async Task WriteLabFileAsync(FileInfo hydrateFile, string labPath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(hydrateFile.FullName);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!document.RootElement.TryGetProperty("sentences", out var sentencesElement))
        {
            throw new InvalidOperationException("Hydrate JSON is missing sentences array");
        }

        var lines = new List<string>();

        foreach (var sentence in sentencesElement.EnumerateArray())
        {
            var script = sentence.TryGetProperty("scriptText", out var scriptProp) ? scriptProp.GetString() : null;
            var text = string.IsNullOrWhiteSpace(script) && sentence.TryGetProperty("bookText", out var bookProp)
                ? bookProp.GetString()
                : script;

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            lines.Add(text.Trim());
        }

        var labContent = string.Join(Environment.NewLine, lines);
        await File.WriteAllTextAsync(labPath, labContent, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
    }

    private static bool EnsureSuccess(string stage, MfaCommandResult result, bool allowFailure = false)
    {
        foreach (var line in result.StdOut)
        {
            Log.Debug("{Stage}> {Line}", stage, line);
        }

        foreach (var line in result.StdErr)
        {
            Log.Warn("{Stage}! {Line}", stage, line);
        }

        Log.Info("{Stage} command: {Command}", stage, result.Command);

        if (result.ExitCode != 0)
        {
            if (!allowFailure)
            {
                throw new InvalidOperationException($"{stage} failed with exit code {result.ExitCode} (command: {result.Command})");
            }

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
            return Directory.EnumerateFiles(directory, "oovs_found*.txt", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to probe for OOV list: {Message}", ex.Message);
            return null;
        }
    }

    private static string? CreateSanitizedOovList(string mfaDir, string chapterStem, string rawOovPath)
    {
        var cleanedPath = Path.Combine(mfaDir, $"{chapterStem}.oov.cleaned.txt");

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
            Log.Warn("Unable to sanitize OOV list {Path}: {Message}", rawOovPath, ex.Message);
            return null;
        }
    }

    private static void CleanupMfaArtifacts(string mfaDir, string chapterStem)
    {
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
                Log.Warn("Failed to delete stale MFA artifact {Path}: {Message}", path, ex.Message);
            }
        }

        TryDelete(Path.Combine(mfaDir, $"{chapterStem}.g2p.txt"));
        TryDelete(Path.Combine(mfaDir, $"{chapterStem}.dictionary.zip"));
        TryDelete(Path.Combine(mfaDir, $"{chapterStem}.oov.cleaned.txt"));

        foreach (var file in Directory.EnumerateFiles(mfaDir, "oovs_found*.txt", SearchOption.TopDirectoryOnly))
        {
            TryDelete(file);
        }

        TryDelete(Path.Combine(mfaDir, "oov_counts_english_mfa.txt"));
        TryDelete(Path.Combine(mfaDir, "utterance_oovs.txt"));
    }
}
