using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using Ams.Cli.Services;
using Ams.Cli.Utilities;
using Ams.Core.Alignment.Mfa;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Book;
using Ams.Core.Common;
using Ams.Core.Hydrate;
using Ams.Core.Prosody;
using Spectre.Console;

namespace Ams.Cli.Commands;

public static class PipelineCommand
{
    public static Command Create()
    {
        var pipeline = new Command("pipeline", "Run the end-to-end chapter pipeline");
        pipeline.AddCommand(CreateRun());
        pipeline.AddCommand(CreatePrepCommand());
        pipeline.AddCommand(CreateStatsCommand());
        return pipeline;
    }

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private const string DefaultBatchFolderName = "Batch 2";

    private static readonly JsonSerializerOptions StatsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static Command CreatePrepCommand()
    {
        var cmd = new Command("prep", "Preparation utilities for batch handoff");
        cmd.AddCommand(CreatePrepStageCommand());
        cmd.AddCommand(CreatePrepResetCommand());
        return cmd;
    }

    private static Command CreateStatsCommand()
    {
        var cmd = new Command("stats", "Display audio and prosody statistics for pipeline artifacts");

        var workDirOption = new Option<DirectoryInfo?>(
            "--work-dir",
            () => null,
            "Root directory containing chapter folders (defaults to the REPL working directory or current directory)");

        var bookIndexOption = new Option<FileInfo?>(
            "--book-index",
            () => null,
            "Optional path to book-index.json (defaults to <work-dir>/book-index.json if present)");

        var chapterOption = new Option<string?>(
            "--chapter",
            () => null,
            "Specific chapter directory to analyze (folder name under work-dir)");

        var allOption = new Option<bool>(
            "--all",
            () => false,
            "Analyze every chapter directory detected under work-dir");

        cmd.AddOption(workDirOption);
        cmd.AddOption(bookIndexOption);
        cmd.AddOption(chapterOption);
        cmd.AddOption(allOption);

        cmd.SetHandler(context =>
        {
            var cancellationToken = context.GetCancellationToken();
            var workDir = context.ParseResult.GetValueForOption(workDirOption);
            var bookIndex = context.ParseResult.GetValueForOption(bookIndexOption);
            var chapter = context.ParseResult.GetValueForOption(chapterOption);
            var all = context.ParseResult.GetValueForOption(allOption);

            try
            {
                RunStats(workDir, bookIndex, chapter, all, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Log.Warn("pipeline stats cancelled");
                context.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "pipeline stats command failed");
                context.ExitCode = 1;
            }
        });

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
        var verboseOption = new Option<bool>("--verbose", () => false, "Enable verbose logging for pipeline stages (roomtone gap analysis, ASR, etc.)");

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
        cmd.AddOption(verboseOption);

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
            var verbose = context.ParseResult.GetValueForOption(verboseOption);

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
                    verbose,
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
        bool verbose,
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

        bool bookIndexExists = bookIndexFile.Exists;

        if (forceIndex || !bookIndexExists)
        {
            Log.Info(forceIndex ? "Rebuilding book index at {BookIndexFile}" : "Building book index at {BookIndexFile}", bookIndexFile.FullName);
            await BuildIndexCommand.BuildBookIndexAsync(
                bookFile,
                bookIndexFile,
                forceIndex,
                new BookIndexOptions { AverageWpm = avgWpm },
                noCache: false);
            bookIndexExists = bookIndexFile.Exists;
        }
        else
        {
            Log.Info("Using cached book index at {BookIndexFile}", bookIndexFile.FullName);
        }

        if (!bookIndexExists)
        {
            throw new InvalidOperationException($"Book index file missing or inaccessible: {bookIndexFile.FullName}");
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
                hydrateFile.Refresh();
                var fallbackTexts = MfaTimingMerger.BuildFallbackTextMap(hydrateFile);
                MfaTimingMerger.MergeTimings(txFile, asrFile, textGridFile, fallbackTexts);
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
            await AudioCommand.RunRenderAsync(txFile, treatedWav, sampleRate, bitDepth, fadeMs, toneDb, emitDiagnostics, adaptiveGain, verbose, gapLeftThresholdDb, gapRightThresholdDb, gapStepMs, gapBackoffMs);
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

    private static void RunStats(DirectoryInfo? workDirOption, FileInfo? bookIndexOption, string? chapterName, bool analyzeAll, CancellationToken cancellationToken)
    {
        var root = CommandInputResolver.ResolveDirectory(workDirOption);
        root.Refresh();
        if (!root.Exists)
        {
            Log.Error("Work directory not found: {Directory}", root.FullName);
            return;
        }

        FileInfo? bookIndexFile = bookIndexOption ?? new FileInfo(Path.Combine(root.FullName, "book-index.json"));
        if (bookIndexFile is not null && !bookIndexFile.Exists)
        {
            if (bookIndexOption is not null)
            {
                Log.Warn("Book index not found at {Path}", bookIndexFile.FullName);
            }
            bookIndexFile = null;
        }

        BookIndex? bookIndex = null;
        if (bookIndexFile is not null)
        {
            try
            {
                bookIndex = LoadJson<BookIndex>(bookIndexFile);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to load book index {Path}: {Message}", bookIndexFile.FullName, ex.Message);
                bookIndexFile = null;
            }
        }

        if (bookIndex is null)
        {
            Log.Warn("Book index unavailable; prosody statistics will be skipped.");
        }

        var chapterDirs = ResolveChapterDirectories(root, chapterName, analyzeAll);
        if (chapterDirs.Count == 0)
        {
            Log.Warn("No chapter directories found under {Root}", root.FullName);
            return;
        }

        var statsList = new List<ChapterStats>();
        double totalAudioSec = 0;

        foreach (var chapterDir in chapterDirs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var stats = ComputeChapterStats(chapterDir, bookIndex, bookIndexFile);
                if (stats is null)
                {
                    continue;
                }

                statsList.Add(stats);
                totalAudioSec += stats.Audio.LengthSec;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to compute stats for {Chapter}: {Message}", chapterDir.Name, ex.Message);
            }
        }

        if (statsList.Count == 0)
        {
            Log.Warn("No statistics could be generated.");
            return;
        }

        PrintStatsReport(root, bookIndexFile, bookIndex, statsList, totalAudioSec);
    }

    private static List<DirectoryInfo> ResolveChapterDirectories(DirectoryInfo root, string? chapterName, bool analyzeAll)
    {
        var chapters = new List<DirectoryInfo>();

        if (!string.IsNullOrWhiteSpace(chapterName))
        {
            var explicitDir = new DirectoryInfo(Path.Combine(root.FullName, chapterName));
            if (!explicitDir.Exists)
            {
                Log.Warn("Chapter directory not found: {Directory}", explicitDir.FullName);
            }
            else
            {
                chapters.Add(explicitDir);
            }

            return chapters;
        }

        var candidates = root.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
            .Where(LooksLikeChapterDirectory)
            .OrderBy(directory => directory.Name, PathComparer)
            .ToList();

        if (candidates.Count == 0)
        {
            return chapters;
        }

        if (analyzeAll || candidates.Count == 1)
        {
            chapters.AddRange(candidates);
            return chapters;
        }

        Log.Warn("Multiple chapter directories detected under {Root}. Use --chapter <name> or --all.", root.FullName);
        return chapters;
    }

    private static ChapterStats? ComputeChapterStats(DirectoryInfo chapterDir, BookIndex? bookIndex, FileInfo? bookIndexFile)
    {
        var txFile = chapterDir.EnumerateFiles("*.align.tx.json", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (txFile is null)
        {
            Log.Warn("Skipping {Chapter}: transcript index not found", chapterDir.FullName);
            return null;
        }

        var stem = ExtractChapterStem(Path.GetFileNameWithoutExtension(txFile.Name));
        var hydrateFile = new FileInfo(Path.Combine(chapterDir.FullName, $"{stem}.align.hydrate.json"));
        var textGridFile = new FileInfo(Path.Combine(chapterDir.FullName, "alignment", "mfa", $"{stem}.TextGrid"));

        var audioCandidates = new[]
        {
            new FileInfo(Path.Combine(chapterDir.FullName, $"{stem}.treated.pause-adjusted.wav")),
            new FileInfo(Path.Combine(chapterDir.FullName, $"{stem}.pause-adjusted.wav")),
            new FileInfo(Path.Combine(chapterDir.FullName, $"{stem}.treated.wav")),
            new FileInfo(Path.Combine(chapterDir.FullName, $"{stem}.wav"))
        };

        var audioFile = audioCandidates.FirstOrDefault(file => file.Exists);
        if (audioFile is null)
        {
            Log.Warn("Skipping {Chapter}: treated audio not found", chapterDir.FullName);
            return null;
        }

        var audioStats = ComputeAudioStats(audioFile);

        PauseStatsSet? prosodyStats = null;
        if (bookIndex is not null && hydrateFile.Exists)
        {
            try
            {
                var transcript = LoadJson<TranscriptIndex>(txFile);
                var hydrated = LoadJson<HydratedTranscript>(hydrateFile);
                var silences = LoadMfaSilences(textGridFile);
                var pauseMap = PauseMapBuilder.Build(transcript, bookIndex, hydrated, PausePolicyPresets.House(), silences, includeAllIntraSentenceGaps: true);
                prosodyStats = pauseMap.Stats;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to compute prosody stats for {Chapter}: {Message}", chapterDir.FullName, ex.Message);
            }
        }

        return new ChapterStats(chapterDir.Name, audioStats, prosodyStats);
    }

    private static void PrintStatsReport(DirectoryInfo root, FileInfo? bookIndexFile, BookIndex? bookIndex, IReadOnlyList<ChapterStats> chapters, double totalAudioSec)
    {
        AnsiConsole.MarkupLine($"[bold]Pipeline Statistics[/] ({root.FullName})");

        if (bookIndexFile is not null)
        {
            AnsiConsole.MarkupLine($"Book Index: {bookIndexFile.FullName}");
        }

        if (bookIndex is not null)
        {
            var totals = bookIndex.Totals;
            var bookTable = new Table().AddColumn("Metric").AddColumn("Value");
            bookTable.AddRow("Words", totals.Words.ToString("N0", CultureInfo.InvariantCulture));
            bookTable.AddRow("Sentences", totals.Sentences.ToString("N0", CultureInfo.InvariantCulture));
            bookTable.AddRow("Paragraphs", totals.Paragraphs.ToString("N0", CultureInfo.InvariantCulture));
            bookTable.AddRow("Estimated Duration", FormatDuration(totals.EstimatedDurationSec));
            bookTable.AddRow("Total Audio (analyzed)", FormatDuration(totalAudioSec));

            if (totals.EstimatedDurationSec > 0)
            {
                var delta = totalAudioSec - totals.EstimatedDurationSec;
                bookTable.AddRow("Audio - Estimate", $"{FormatDuration(delta)} ({delta:+0.##;-0.##;0} s)");
            }

            AnsiConsole.Write(bookTable);
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Book index not available; book-level statistics omitted.[/]");
        }

        AnsiConsole.MarkupLine($"Chapters analyzed: {chapters.Count}");

        foreach (var chapter in chapters.OrderBy(c => c.Chapter, StringComparer.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"\n[bold yellow]{chapter.Chapter}[/]");

            var audioTable = CreateAudioTable(chapter.Audio);
            AnsiConsole.MarkupLine("[bold]Audio[/]");
            AnsiConsole.Write(audioTable);

            if (chapter.Prosody is not null)
            {
                var prosodyTable = CreateProsodyTable(chapter.Prosody);
                if (prosodyTable is not null)
                {
                    AnsiConsole.MarkupLine("[bold]Prosody[/]");
                    AnsiConsole.Write(prosodyTable);
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]No pause intervals detected for this chapter.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Prosody statistics unavailable (book index missing or failed to load).[/]");
            }
        }
    }

    private static Table CreateAudioTable(AudioStats audio)
    {
        var table = new Table().AddColumn("Metric").AddColumn("Value");
        table.AddRow("Length", FormatDuration(audio.LengthSec));
        table.AddRow("Sample Peak", FormatDb(audio.SamplePeak));
        table.AddRow("True Peak", FormatDb(audio.TruePeak));
        table.AddRow("Overall RMS", FormatDb(audio.OverallRms));
        table.AddRow("Window RMS Max (0.5s)", FormatDb(audio.MaxWindowRms));
        table.AddRow("Window RMS Min (0.5s)", FormatDb(audio.MinWindowRms));
        return table;
    }

    private static Table? CreateProsodyTable(PauseStatsSet stats)
    {
        var table = new Table()
            .AddColumn("Class")
            .AddColumn("Count")
            .AddColumn("Min (s)")
            .AddColumn("Median (s)")
            .AddColumn("Max (s)")
            .AddColumn("Mean (s)")
            .AddColumn("Total (s)");

        var hasRows = false;
        foreach (var (pauseClass, pauseStats) in EnumerateStats(stats))
        {
            if (pauseStats.Count == 0)
            {
                continue;
            }

            hasRows = true;
            table.AddRow(
                pauseClass.ToString(),
                pauseStats.Count.ToString(CultureInfo.InvariantCulture),
                pauseStats.Min.ToString("F3", CultureInfo.InvariantCulture),
                pauseStats.Median.ToString("F3", CultureInfo.InvariantCulture),
                pauseStats.Max.ToString("F3", CultureInfo.InvariantCulture),
                pauseStats.Mean.ToString("F3", CultureInfo.InvariantCulture),
                pauseStats.Total.ToString("F3", CultureInfo.InvariantCulture));
        }

        return hasRows ? table : null;
    }

    private static IEnumerable<(PauseClass Class, PauseStats Stats)> EnumerateStats(PauseStatsSet stats)
    {
        yield return (PauseClass.Comma, stats.Comma);
        yield return (PauseClass.Sentence, stats.Sentence);
        yield return (PauseClass.Paragraph, stats.Paragraph);
        yield return (PauseClass.ChapterHead, stats.ChapterHead);
        yield return (PauseClass.PostChapterRead, stats.PostChapterRead);
        yield return (PauseClass.Tail, stats.Tail);
        yield return (PauseClass.Other, stats.Other);
    }

    private static string FormatDuration(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return ts.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
    }

    private static string FormatDb(double amplitude)
    {
        if (amplitude <= 0)
        {
            return "-∞ dBFS";
        }

        var db = 20.0 * Math.Log10(amplitude);
        return db.ToString("F2", CultureInfo.InvariantCulture) + " dBFS";
    }

    private static AudioStats ComputeAudioStats(FileInfo audioFile)
    {
        var buffer = WavIo.ReadPcmOrFloat(audioFile.FullName);
        var totalSamples = buffer.Length;
        var channels = buffer.Channels;
        if (totalSamples == 0 || channels == 0)
        {
            return new AudioStats(0, 0, 0, 0, 0, 0);
        }

        var sampleRate = buffer.SampleRate;
        var channelData = buffer.Planar;

        var perSampleMeanSquare = new double[totalSamples];
        double totalSquares = 0;
        double samplePeak = 0;
        double truePeak = 0;

        for (int ch = 0; ch < channels; ch++)
        {
            var samples = channelData[ch];
            double previous = 0;
            for (int i = 0; i < totalSamples; i++)
            {
                double sample = samples[i];
                double abs = Math.Abs(sample);
                if (abs > samplePeak)
                {
                    samplePeak = abs;
                }

                double square = sample * sample;
                perSampleMeanSquare[i] += square;
                totalSquares += square;

                if (i > 0)
                {
                    double s0 = previous;
                    double s1 = sample;
                    for (int step = 1; step <= 3; step++)
                    {
                        double t = step / 4.0;
                        double interp = s0 + (s1 - s0) * t;
                        double interpAbs = Math.Abs(interp);
                        if (interpAbs > truePeak)
                        {
                            truePeak = interpAbs;
                        }
                    }
                }

                previous = sample;
            }
        }

        truePeak = Math.Max(truePeak, samplePeak);

        for (int i = 0; i < totalSamples; i++)
        {
            perSampleMeanSquare[i] /= channels;
        }

        double overallRms = Math.Sqrt(totalSquares / (channels * (double)totalSamples));

        int windowSamples = Math.Max(1, (int)Math.Round(sampleRate * 0.5));
        if (windowSamples > totalSamples)
        {
            windowSamples = totalSamples;
        }

        double minWindowRms = double.PositiveInfinity;
        double maxWindowRms = double.NegativeInfinity;

        for (int start = 0; start < totalSamples; start += windowSamples)
        {
            int end = Math.Min(totalSamples, start + windowSamples);
            if (end <= start)
            {
                continue;
            }

            double sum = 0;
            for (int i = start; i < end; i++)
            {
                sum += perSampleMeanSquare[i];
            }

            double meanSquare = sum / (end - start);
            double rms = Math.Sqrt(meanSquare);
            if (rms < minWindowRms)
            {
                minWindowRms = rms;
            }
            if (rms > maxWindowRms)
            {
                maxWindowRms = rms;
            }
        }

        if (double.IsPositiveInfinity(minWindowRms))
        {
            minWindowRms = overallRms;
        }
        if (double.IsNegativeInfinity(maxWindowRms))
        {
            maxWindowRms = overallRms;
        }

        double lengthSec = totalSamples / (double)sampleRate;

        return new AudioStats(lengthSec, samplePeak, truePeak, overallRms, minWindowRms, maxWindowRms);
    }

    private static IReadOnlyList<(double Start, double End)> LoadMfaSilences(FileInfo textGridFile)
    {
        if (!textGridFile.Exists)
        {
            return Array.Empty<(double, double)>();
        }

        try
        {
            return TextGridParser.ParseWordIntervals(textGridFile.FullName)
                .Where(interval => IsSilenceLabel(interval.Text) && interval.End > interval.Start)
                .Select(interval => (interval.Start, interval.End))
                .ToList();
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to parse TextGrid for silences {Path}: {Message}", textGridFile.FullName, ex.Message);
            return Array.Empty<(double, double)>();
        }
    }

    private static bool IsSilenceLabel(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        text = text.Trim();
        return text.Length switch
        {
            0 => true,
            2 when text.Equals("sp", StringComparison.OrdinalIgnoreCase) => true,
            3 when text.Equals("sil", StringComparison.OrdinalIgnoreCase) => true,
            _ => text.Equals("<sil>", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string ExtractChapterStem(string nameWithoutExtension)
    {
        var stem = nameWithoutExtension;

        const string alignTxSuffix = ".align.tx";
        if (stem.EndsWith(alignTxSuffix, StringComparison.OrdinalIgnoreCase))
        {
            stem = stem[..^alignTxSuffix.Length];
        }
        else if (stem.EndsWith(".tx", StringComparison.OrdinalIgnoreCase))
        {
            stem = stem[..^3];
        }

        if (stem.EndsWith(".align", StringComparison.OrdinalIgnoreCase))
        {
            stem = stem[..^".align".Length];
        }

        return stem;
    }

    private static T LoadJson<T>(FileInfo file)
    {
        using var stream = file.OpenRead();
        var payload = JsonSerializer.Deserialize<T>(stream, StatsJsonOptions);
        return payload ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from {file.FullName}");
    }

    private sealed record AudioStats(double LengthSec, double SamplePeak, double TruePeak, double OverallRms, double MinWindowRms, double MaxWindowRms);

    private sealed record ChapterStats(string Chapter, AudioStats Audio, PauseStatsSet? Prosody);

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
