using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading;
using Ams.Cli.Services;
using Ams.Core.Alignment.Mfa;
using Ams.Core.Book;
using Ams.Core.Common;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class PipelineCommand
{
    public static Command Create()
    {
        var pipeline = new Command("pipeline", "Run the end-to-end chapter pipeline");
        pipeline.AddCommand(CreateRun());
        pipeline.AddCommand(CreatePrepCommand());
        return pipeline;
    }

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private const string DefaultBatchFolderName = "Batch 2";

    private static Command CreatePrepCommand()
    {
        var cmd = new Command("prep", "Preparation utilities for batch handoff");
        cmd.AddCommand(CreatePrepStageCommand());
        cmd.AddCommand(CreatePrepResetCommand());
        return cmd;
    }

    private static Command CreatePrepStageCommand()
    {
        var cmd = new Command("stage", "Collect treated WAVs into a batch folder for delivery");

        var rootOption = new Option<DirectoryInfo?>(
            "--root",
            () => null,
            "Root directory containing chapter folders (defaults to the REPL working directory or current directory)");
        rootOption.AddAlias("-r");

        var outputOption = new Option<DirectoryInfo?>(
            "--output",
            "Destination directory for staged files (defaults to <root>/Batch 2)");
        outputOption.AddAlias("-o");

        var overwriteOption = new Option<bool>(
            "--overwrite",
            () => false,
            "Overwrite existing files in the destination folder");

        cmd.AddOption(rootOption);
        cmd.AddOption(outputOption);
        cmd.AddOption(overwriteOption);

        cmd.SetHandler(context =>
        {
            var cancellationToken = context.GetCancellationToken();

            var root = CommandInputResolver.ResolveDirectory(context.ParseResult.GetValueForOption(rootOption));
            root.Refresh();
            if (!root.Exists)
            {
                throw new DirectoryNotFoundException($"Root directory not found: {root.FullName}");
            }

            var explicitOutput = context.ParseResult.GetValueForOption(outputOption);
            var destination = explicitOutput ?? new DirectoryInfo(Path.Combine(root.FullName, DefaultBatchFolderName));
            EnsureDirectory(destination.FullName);
            destination.Refresh();

            var overwrite = context.ParseResult.GetValueForOption(overwriteOption);

            var destNormalized = NormalizeDirectoryPath(destination.FullName);

            var treatedFiles = Directory.EnumerateFiles(root.FullName, "*.treated.wav", SearchOption.AllDirectories)
                .Where(path => !IsWithinDirectory(path, destNormalized))
                .Select(path => new FileInfo(path))
                .OrderBy(file => file.FullName, PathComparer)
                .ToList();

            if (treatedFiles.Count == 0)
            {
                Log.Info("No treated WAV files found under {Root}", root.FullName);
                return;
            }

            Log.Info("Staging {Count} treated file(s) from {Root} to {Destination}", treatedFiles.Count, root.FullName, destination.FullName);

            var stagedCount = 0;
            foreach (var file in treatedFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var targetName = GetStagedFileName(file);
                var targetPath = Path.Combine(destination.FullName, targetName);

                if (!overwrite && File.Exists(targetPath))
                {
                    Log.Warn("Skipping {Source}; destination already exists at {Destination} (use --overwrite to replace)", file.FullName, targetPath);
                    continue;
                }

                File.Copy(file.FullName, targetPath, overwrite);
                stagedCount++;
            }

            Log.Info("Staged {Count} file(s) into {Destination}", stagedCount, destination.FullName);
        });

        return cmd;
    }

    private static Command CreatePrepResetCommand()
    {
        var cmd = new Command("reset", "Remove generated chapter artifacts from the working directory");

        var rootOption = new Option<DirectoryInfo?>(
            "--root",
            () => null,
            "Root directory containing chapter folders (defaults to the REPL working directory or current directory)");
        rootOption.AddAlias("-r");

        var hardOption = new Option<bool>(
            "--hard",
            () => false,
            "Delete everything under the root except manuscript DOCX files");

        cmd.AddOption(rootOption);
        cmd.AddOption(hardOption);

        cmd.SetHandler(context =>
        {
            var cancellationToken = context.GetCancellationToken();

            var root = CommandInputResolver.ResolveDirectory(context.ParseResult.GetValueForOption(rootOption));
            root.Refresh();
            if (!root.Exists)
            {
                throw new DirectoryNotFoundException($"Root directory not found: {root.FullName}");
            }

            var hard = context.ParseResult.GetValueForOption(hardOption);

            if (hard)
            {
                Log.Warn("Hard reset: deleting all generated content under {Root} (DOCX files preserved)", root.FullName);
                PerformHardReset(root, cancellationToken);
            }
            else
            {
                Log.Info("Resetting chapter directories under {Root}", root.FullName);
                PerformSoftReset(root, cancellationToken);
            }
        });

        return cmd;
    }

    private static Command CreateRun()
    {
        var cmd = new Command("run", "Build index, run ASR, align, hydrate, and apply roomtone in one pass");

        var bookOption = new Option<FileInfo?>("--book", "Path to the book manuscript (DOCX/TXT/etc.)");
        bookOption.AddAlias("-b");

        var audioOption = new Option<FileInfo?>("--audio", "Path to the chapter audio WAV");
        audioOption.AddAlias("-a");

        var workDirOption = new Option<DirectoryInfo?>("--work-dir", () => null, "Working directory for generated artifacts");
        var bookIndexOption = new Option<FileInfo?>("--book-index", () => null, "Existing/target BookIndex JSON path (defaults to work-dir/book-index.json)");
        var chapterIdOption = new Option<string?>("--chapter-id", () => null, "Override output stem (defaults to audio file name)");
        var forceOption = new Option<bool>("--force", () => false, "Re-run all stages even if outputs are already present");
        forceOption.AddAlias("-f");
        var forceIndexOption = new Option<bool>("--force-index", () => false, "Rebuild book index even if it already exists");
        var avgWpmOption = new Option<double>("--avg-wpm", () => 200.0, "Average WPM used for duration estimation when indexing");

        var asrServiceOption = new Option<string>("--asr-service", () => "http://localhost:8000", "ASR service URL");
        asrServiceOption.AddAlias("-s");
        var asrModelOption = new Option<string?>("--asr-model", () => null, "Optional ASR model identifier");
        asrModelOption.AddAlias("-m");
        var asrLanguageOption = new Option<string>("--language", () => "en", "ASR language code");
        asrLanguageOption.AddAlias("-l");

        var sampleRateOption = new Option<int>("--sample-rate", () => 44100, "Roomtone output sample rate (Hz)");
        var bitDepthOption = new Option<int>("--bit-depth", () => 32, "Roomtone output bit depth");
        var fadeMsOption = new Option<double>("--fade-ms", () => 10.0, "Crossfade length for roomtone boundaries (ms)");
        var toneDbOption = new Option<double>("--tone-gain-db", () => -60.0, "Target RMS level for roomtone (dBFS)");
        var diagnosticsOption = new Option<bool>("--emit-diagnostics", () => false, "Emit diagnostic WAVs during roomtone rendering");
        var adaptiveGainOption = new Option<bool>("--adaptive-gain", () => false, "Scale roomtone seed to match target RMS");
        var gapLeftThresholdOption = new Option<double>("--gap-left-threshold-db", () => -30.0, "RMS threshold (dBFS) used to detect silence on the left side of gaps");
        var gapRightThresholdOption = new Option<double>("--gap-right-threshold-db", () => -30.0, "RMS threshold (dBFS) used to detect silence on the right side of gaps");
        var gapStepOption = new Option<double>("--gap-step-ms", () => 5.0, "Step size (ms) when probing gap boundaries");
        var gapBackoffOption = new Option<double>("--gap-backoff-ms", () => 5.0, "Backoff amount (ms) applied after a silent window is detected");

        cmd.AddOption(bookOption);
        cmd.AddOption(audioOption);
        cmd.AddOption(workDirOption);
        cmd.AddOption(bookIndexOption);
        cmd.AddOption(chapterIdOption);
        cmd.AddOption(forceOption);
        cmd.AddOption(forceIndexOption);
        cmd.AddOption(avgWpmOption);
        cmd.AddOption(asrServiceOption);
        cmd.AddOption(asrModelOption);
        cmd.AddOption(asrLanguageOption);
        cmd.AddOption(sampleRateOption);
        cmd.AddOption(bitDepthOption);
        cmd.AddOption(fadeMsOption);
        cmd.AddOption(toneDbOption);
        cmd.AddOption(diagnosticsOption);
        cmd.AddOption(adaptiveGainOption);
        cmd.AddOption(gapLeftThresholdOption);
        cmd.AddOption(gapRightThresholdOption);
        cmd.AddOption(gapStepOption);
        cmd.AddOption(gapBackoffOption);

        cmd.SetHandler(async context =>
        {
            var cancellationToken = context.GetCancellationToken();
            var bookFile = CommandInputResolver.ResolveBookSource(context.ParseResult.GetValueForOption(bookOption));
            var audioFile = CommandInputResolver.RequireAudio(context.ParseResult.GetValueForOption(audioOption));
            var workDir = context.ParseResult.GetValueForOption(workDirOption);
            var bookIndex = context.ParseResult.GetValueForOption(bookIndexOption) ?? CommandInputResolver.ResolveBookIndex(null, mustExist: false);
            var chapterId = context.ParseResult.GetValueForOption(chapterIdOption) ?? Path.GetFileNameWithoutExtension(audioFile.Name);
            var forceAll = context.ParseResult.GetValueForOption(forceOption);
            var forceIndex = context.ParseResult.GetValueForOption(forceIndexOption);
            var avgWpm = context.ParseResult.GetValueForOption(avgWpmOption);
            var asrService = context.ParseResult.GetValueForOption(asrServiceOption) ?? "http://localhost:8000";
            var asrModel = context.ParseResult.GetValueForOption(asrModelOption);
            var asrLanguage = context.ParseResult.GetValueForOption(asrLanguageOption) ?? "en";
            var sampleRate = context.ParseResult.GetValueForOption(sampleRateOption);
            var bitDepth = context.ParseResult.GetValueForOption(bitDepthOption);
            var fadeMs = context.ParseResult.GetValueForOption(fadeMsOption);
            var toneDb = context.ParseResult.GetValueForOption(toneDbOption);
            var emitDiagnostics = context.ParseResult.GetValueForOption(diagnosticsOption);
            var adaptiveGain = context.ParseResult.GetValueForOption(adaptiveGainOption);
            var gapLeftThreshold = context.ParseResult.GetValueForOption(gapLeftThresholdOption);
            var gapRightThreshold = context.ParseResult.GetValueForOption(gapRightThresholdOption);
            var gapStep = context.ParseResult.GetValueForOption(gapStepOption);
            var gapBackoff = context.ParseResult.GetValueForOption(gapBackoffOption);

            try
            {
                await RunPipelineAsync(
                    bookFile,
                    audioFile,
                    workDir,
                    bookIndex,
                    chapterId,
                    forceIndex,
                    forceAll,
                    avgWpm,
                    asrService,
                    asrModel,
                    asrLanguage,
                    sampleRate,
                    bitDepth,
                    fadeMs,
                    toneDb,
                    emitDiagnostics,
                    adaptiveGain,
                    gapLeftThreshold,
                    gapRightThreshold,
                    gapStep,
                    gapBackoff,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "pipeline run command failed");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunPipelineAsync(
        FileInfo bookFile,
        FileInfo audioFile,
        DirectoryInfo? workDirOption,
        FileInfo? bookIndexOverride,
        string? chapterIdOverride,
        bool forceIndex,
        bool force,
        double avgWpm,
        string asrService,
        string? asrModel,
        string asrLanguage,
        int sampleRate,
        int bitDepth,
        double fadeMs,
        double toneDb,
        bool emitDiagnostics,
        bool adaptiveGain,
        double gapLeftThresholdDb,
        double gapRightThresholdDb,
        double gapStepMs,
        double gapBackoffMs,
        CancellationToken cancellationToken)
    {
        if (!bookFile.Exists)
        {
            throw new FileNotFoundException($"Book file not found: {bookFile.FullName}");
        }

        if (!audioFile.Exists)
        {
            throw new FileNotFoundException($"Audio file not found: {audioFile.FullName}");
        }

        var workDirPath = workDirOption?.FullName ?? audioFile.Directory?.FullName ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(workDirPath);

        var chapterStem = MakeSafeFileStem(string.IsNullOrWhiteSpace(chapterIdOverride)
            ? Path.GetFileNameWithoutExtension(audioFile.Name)
            : chapterIdOverride!);
        var chapterDir = Path.Combine(workDirPath, chapterStem);
        EnsureDirectory(chapterDir);
        var chapterDirInfo = new DirectoryInfo(chapterDir);

        var bookIndexFile = bookIndexOverride ?? new FileInfo(Path.Combine(workDirPath, "book-index.json"));
        EnsureDirectory(bookIndexFile.DirectoryName);

        var asrFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.asr.json"));
        var anchorsFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.align.anchors.json"));
        var txFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.align.tx.json"));
        var hydrateFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.align.hydrate.json"));
        var treatedWav = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.treated.wav"));
        var textGridFile = new FileInfo(Path.Combine(chapterDir, "alignment", "mfa", $"{chapterStem}.TextGrid"));

        forceIndex |= force;

        Log.Info("=== AMS Pipeline ===");
        Log.Info("Book={BookFile}", bookFile.FullName);
        Log.Info("Audio={AudioFile}", audioFile.FullName);
        Log.Info("WorkDir={WorkDir}", workDirPath);
        Log.Info("Chapter={Chapter}", chapterStem);
        Log.Info("ChapterDir={ChapterDir}", chapterDir);

        if (forceIndex || !bookIndexFile.Exists)
        {
            Log.Info(forceIndex ? "Rebuilding book index at {BookIndexFile}" : "Building book index at {BookIndexFile}", bookIndexFile.FullName);
            await BuildIndexCommand.BuildBookIndexAsync(
                bookFile,
                bookIndexFile,
                forceIndex,
                new BookIndexOptions { AverageWpm = avgWpm },
                noCache: false);
        }
        else
        {
            Log.Info("Using cached book index at {BookIndexFile}", bookIndexFile.FullName);
        }

        if (!bookIndexFile.Exists)
        {
            throw new InvalidOperationException($"Book index file missing after build: {bookIndexFile.FullName}");
        }

        EnsureDirectory(asrFile.DirectoryName);
        asrFile.Refresh();
        if (!force && asrFile.Exists)
        {
            Log.Info("Skipping ASR stage; {AsrFile} already exists (pass --force to rerun)", asrFile.FullName);
        }
        else
        {
            Log.Info("Running ASR stage");
            await AsrCommand.RunAsrAsync(audioFile, asrFile, asrService, asrModel, asrLanguage);
            asrFile.Refresh();
        }

        anchorsFile.Refresh();
        if (!force && anchorsFile.Exists)
        {
            Log.Info("Skipping anchor selection; {AnchorsFile} already exists (pass --force to rerun)", anchorsFile.FullName);
        }
        else
        {
            Log.Info("Selecting anchors");
            asrFile.Refresh();
            await AlignCommand.RunAnchorsAsync(
                bookIndexFile,
                asrFile,
                anchorsFile,
                detectSection: true,
                ngram: 3,
                targetPerTokens: 50,
                minSeparation: 100,
                crossSentences: false,
                domainStopwords: true,
                asrPrefixTokens: 8,
                emitWindows: false);
            anchorsFile.Refresh();
        }

        txFile.Refresh();
        if (!force && txFile.Exists)
        {
            Log.Info("Skipping transcript index; {TranscriptFile} already exists (pass --force to rerun)", txFile.FullName);
        }
        else
        {
            Log.Info("Generating transcript index");
            asrFile.Refresh();
            await AlignCommand.RunTranscriptIndexAsync(
                bookIndexFile,
                asrFile,
                audioFile,
                txFile,
                detectSection: true,
                asrPrefixTokens: 8,
                ngram: 3,
                targetPerTokens: 50,
                minSeparation: 100,
                crossSentences: false,
                domainStopwords: true);
            txFile.Refresh();
        }

        hydrateFile.Refresh();
        if (!force && hydrateFile.Exists)
        {
            Log.Info("Skipping hydrate stage; {HydratedFile} already exists (pass --force to rerun)", hydrateFile.FullName);
        }
        else
        {
            Log.Info("Hydrating transcript");
            asrFile.Refresh();
            txFile.Refresh();
            await AlignCommand.RunHydrateTxAsync(bookIndexFile, asrFile, txFile, hydrateFile);
            hydrateFile.Refresh();
        }

        cancellationToken.ThrowIfCancellationRequested();

        textGridFile.Refresh();
        if (!force && textGridFile.Exists)
        {
            Log.Info("Skipping MFA alignment; {TextGridFile} already exists (pass --force to rerun)", textGridFile.FullName);
        }
        else
        {
            Log.Info("Running MFA alignment workflow");
            hydrateFile.Refresh();
            await MfaWorkflow.RunChapterAsync(audioFile, hydrateFile, chapterStem, chapterDirInfo, cancellationToken);
            textGridFile.Refresh();
        }

        if (textGridFile.Exists)
        {
            try
            {
                MfaTimingMerger.MergeTimings(hydrateFile, asrFile, textGridFile);
                MfaTimingMerger.MergeTimings(txFile, asrFile, textGridFile);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to merge MFA timings: {0}", ex);
            }
        }
        else
        {
            Log.Warn("Skipping MFA timing merge because TextGrid not found at {TextGridFile}", textGridFile.FullName);
        }

        treatedWav.Refresh();
        if (!force && treatedWav.Exists)
        {
            Log.Info("Skipping roomtone render; {RoomtoneFile} already exists (pass --force to rerun)", treatedWav.FullName);
        }
        else
        {
            Log.Info("Rendering roomtone");
            txFile.Refresh();
            await AudioCommand.RunRenderAsync(txFile, treatedWav, sampleRate, bitDepth, fadeMs, toneDb, emitDiagnostics, adaptiveGain, verbose: false, gapLeftThresholdDb, gapRightThresholdDb, gapStepMs, gapBackoffMs);
            treatedWav.Refresh();
        }

        Log.Info("=== Outputs ===");
        Log.Info("Book index : {BookIndex}", bookIndexFile.FullName);
        Log.Info("ASR JSON   : {AsrFile}", asrFile.FullName);
        Log.Info("Anchors    : {AnchorsFile}", anchorsFile.FullName);
        Log.Info("Transcript : {TranscriptFile}", txFile.FullName);
        Log.Info("Hydrated   : {HydratedFile}", hydrateFile.FullName);
        Log.Info("Roomtone   : {RoomtoneFile}", treatedWav.FullName);
    }

    private static void PerformSoftReset(DirectoryInfo root, CancellationToken cancellationToken)
    {
        var defaultBatchPath = Path.Combine(root.FullName, DefaultBatchFolderName);
        var skipBatch = NormalizeDirectoryPath(defaultBatchPath);

        var deleted = 0;
        foreach (var directory in root.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalized = NormalizeDirectoryPath(directory.FullName);
            if (normalized.Equals(skipBatch, PathComparison))
            {
                continue;
            }

            if (!LooksLikeChapterDirectory(directory))
            {
                continue;
            }

            try
            {
                directory.Delete(recursive: true);
                deleted++;
                Log.Info("Deleted chapter directory {Directory}", directory.FullName);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to delete {Directory}: {Message}", directory.FullName, ex.Message);
            }
        }

        Log.Info("Soft reset complete. Removed {Count} chapter directorie(s).", deleted);
    }

    private static void PerformHardReset(DirectoryInfo root, CancellationToken cancellationToken)
    {
        // Delete directories first
        foreach (var directory in root.EnumerateDirectories())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                directory.Delete(recursive: true);
                Log.Info("Deleted directory {Directory}", directory.FullName);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to delete {Directory}: {Message}", directory.FullName, ex.Message);
            }
        }

        // Delete files except DOCX
        foreach (var file in root.EnumerateFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                file.Delete();
                Log.Info("Deleted file {File}", file.FullName);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to delete {File}: {Message}", file.FullName, ex.Message);
            }
        }

        Log.Info("Hard reset complete for {Root}", root.FullName);
    }

    private static void EnsureDirectory(string? dir)
    {
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private static string MakeSafeFileStem(string? value)
    {
        const string fallback = "chapter";
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
        }

        var result = builder.ToString().Trim();
        return string.IsNullOrEmpty(result) ? fallback : result;
    }

    private static bool LooksLikeChapterDirectory(DirectoryInfo directory)
    {
        if (directory.Name.Equals(DefaultBatchFolderName, PathComparison))
        {
            return false;
        }

        if (directory.Name.StartsWith(".", StringComparison.Ordinal))
        {
            return false;
        }

        // Quick heuristic: presence of common pipeline artifacts
        var patterns = new[]
        {
            "*.treated.wav",
            "*.asr.json",
            "*.align.*",
            "*.validate.*",
            "*.tx.json",
            "*.wav"
        };

        foreach (var pattern in patterns)
        {
            if (directory.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly).Any())
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeDirectoryPath(string path)
    {
        var full = Path.GetFullPath(path);
        return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }

    private static bool IsWithinDirectory(string candidatePath, string directoryNormalized)
    {
        var candidateFull = Path.GetFullPath(candidatePath);
        return candidateFull.StartsWith(directoryNormalized, PathComparison);
    }

    private static string GetStagedFileName(FileInfo source)
    {
        var stem = Path.GetFileNameWithoutExtension(source.Name);
        const string marker = ".treated";

        if (stem.EndsWith(marker, PathComparison))
        {
            stem = stem[..^marker.Length];
        }
        else if (stem.EndsWith("treated", PathComparison))
        {
            stem = stem[..^"treated".Length];
        }

        return stem + source.Extension;
    }
}
