using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Cli.Utilities;
using Ams.Cli.Repl;
using Ams.Core.Application.Services;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Asr;
using Ams.Core.Processors;
using Ams.Core.Runtime.Documents;
using Ams.Core.Common;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Prosody;
using Spectre.Console;
using AudioSentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Cli.Commands;

public static class PipelineCommand
{
    private const int PipelineStageCount = (int)PipelineStage.Complete;

    private static readonly ProgressColumn[] PipelineProgressColumns =
    {
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn()
    };

    private static void LogStageInfo(bool quiet, string message, params object?[] args)
    {
        if (quiet)
        {
            return;
        }

        Log.Debug(message, args);
    }

    public static Command Create(PipelineService pipelineService)
    {
        ArgumentNullException.ThrowIfNull(pipelineService);

        var pipeline = new Command("pipeline", "Run the end-to-end chapter pipeline");
        pipeline.AddCommand(CreateRun(pipelineService));
        pipeline.AddCommand(CreatePrepCommand());
        pipeline.AddCommand(CreateVerifyCommand());
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

    private static readonly JsonSerializerOptions VerifyJsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Regex PatternTokenRegex = new(@"\{(d{1,2}|um\d+-\d+|um\d+|um\*)([+-]\d+)?\}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static Command CreatePrepCommand()
    {
        var cmd = new Command("prep", "Preparation utilities for batch handoff");
        cmd.AddCommand(CreatePrepStageCommand());
        cmd.AddCommand(CreatePrepRenameCommand());
        cmd.AddCommand(CreatePrepResetCommand());
        return cmd;
    }

    private sealed class PipelineProgressReporter
    {
        private readonly object _sync = new();
        private readonly Dictionary<string, ProgressTask> _tasks;

        private static readonly Dictionary<PipelineStage, (string Label, string Color)> StageStyles = new()
        {
            [PipelineStage.Pending] = ("Queued", "grey"),
            [PipelineStage.BookIndex] = ("Index", "deepskyblue1"),
            [PipelineStage.Asr] = ("ASR", "deepskyblue1"),
            [PipelineStage.Anchors] = ("Anchors", "deepskyblue1"),
            [PipelineStage.Transcript] = ("Transcript", "deepskyblue1"),
            [PipelineStage.Hydrate] = ("Hydrate", "deepskyblue1"),
            [PipelineStage.Mfa] = ("MFA", "lightseagreen"),
            [PipelineStage.Complete] = ("Done", "green")
        };

        public PipelineProgressReporter(ProgressContext context, IReadOnlyList<FileInfo> chapters)
        {
            _tasks = new Dictionary<string, ProgressTask>(StringComparer.OrdinalIgnoreCase);

            foreach (var chapterFile in chapters)
            {
                var chapterId = Path.GetFileNameWithoutExtension(chapterFile.Name);
                var task = context.AddTask(chapterId, autoStart: true, maxValue: PipelineStageCount);
                task.Value = 0;
                task.Description = BuildDescription(chapterId, PipelineStage.Pending, "Queued");
                _tasks[chapterId] = task;
            }
        }

        public void SetQueued(string chapterId)
        {
            Update(chapterId, PipelineStage.Pending, "Queued");
        }

        public void ReportStage(string chapterId, PipelineStage stage, string message)
        {
            Update(chapterId, stage, message);
        }

        public void MarkComplete(string chapterId)
        {
            Update(chapterId, PipelineStage.Complete, "Complete");
            if (_tasks.TryGetValue(chapterId, out var task))
            {
                lock (_sync)
                {
                    task.StopTask();
                }
            }
        }

        public void MarkFailed(string chapterId, string message)
        {
            if (!_tasks.TryGetValue(chapterId, out var task))
            {
                return;
            }

            lock (_sync)
            {
                task.Value = PipelineStageCount;
                task.Description = $"{chapterId,-20} [red]Failed[/] {message}";
                task.StopTask();
            }
        }

        private void Update(string chapterId, PipelineStage stage, string message)
        {
            if (!_tasks.TryGetValue(chapterId, out var task))
            {
                return;
            }

            lock (_sync)
            {
                var clamped = Math.Min((int)stage, PipelineStageCount);
                task.Value = clamped;
                task.Description = BuildDescription(chapterId, stage, message);
            }
        }

        private static string BuildDescription(string chapterId, PipelineStage stage, string message)
        {
            if (!StageStyles.TryGetValue(stage, out var style))
            {
                style = ("", "grey");
            }

            var label = string.IsNullOrWhiteSpace(style.Label)
                ? string.Empty
                : $"[bold {style.Color}]{style.Label}[/]";

            var detail = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            var combined = string.IsNullOrWhiteSpace(label) ? detail : $"{label} {detail}";
            return $"{chapterId,-20} {combined}".TrimEnd();
        }
    }

private static async Task RunPipelineForMultipleChaptersAsync(
    PipelineService pipelineService,
        FileInfo bookFile,
        DirectoryInfo? workDirOption,
        FileInfo? bookIndexOverride,
        bool forceIndex,
        bool force,
        double avgWpm,
        string asrServiceUrl,
        string? asrModel,
        string asrLanguage,
        bool verbose,
        IReadOnlyList<FileInfo> chapterFiles,
        int maxWorkers,
        int maxMfaParallelism,
        ProgressContext? progressContext,
        CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(pipelineService);

        if (chapterFiles is null || chapterFiles.Count == 0)
        {
            Log.Debug("No chapters available for pipeline run.");
            return;
        }

        var existingChapters = chapterFiles
            .Select(file => { file.Refresh(); return file; })
            .Where(file => file.Exists)
            .ToList();

        if (existingChapters.Count == 0)
        {
            Log.Debug("No chapter WAV files exist on disk; aborting pipeline run.");
            return;
        }

        if (existingChapters.Count != chapterFiles.Count)
        {
            Log.Debug("Skipping {Missing} chapter(s) because the WAV file was not found.", chapterFiles.Count - existingChapters.Count);
        }

        PipelineProgressReporter? reporter = progressContext is not null
            ? new PipelineProgressReporter(progressContext, existingChapters)
            : null;
        maxWorkers = maxWorkers <= 0 ? Math.Max(1, Environment.ProcessorCount) : maxWorkers;
        maxMfaParallelism = maxMfaParallelism <= 0 ? Math.Max(1, Environment.ProcessorCount / 2) : maxMfaParallelism;

        Log.Debug(
            "Starting parallel pipeline run for {Count} chapter(s) (maxWorkers={Workers}, maxMfa={Mfa}).",
            existingChapters.Count,
            maxWorkers,
            maxMfaParallelism);

        using var concurrency = PipelineConcurrencyControl.CreateShared(maxMfaParallelism);
        using var workerSemaphore = new SemaphoreSlim(maxWorkers, maxWorkers);
        var errors = new ConcurrentBag<Exception>();

        var tasks = existingChapters.Select(async chapter =>
        {
            var chapterId = Path.GetFileNameWithoutExtension(chapter.Name);
            reporter?.SetQueued(chapterId);

            try
            {
                await workerSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException oce)
            {
                reporter?.MarkFailed(chapterId, "Cancelled");
                errors.Add(new InvalidOperationException($"Pipeline cancelled before starting {chapter.FullName}", oce));
                return;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await RunPipelineAsync(
                    pipelineService,
                    bookFile,
                    chapter,
                    workDirOption,
                    bookIndexOverride,
                    chapterId,
                    forceIndex,
                    force,
                    avgWpm,
                    asrServiceUrl,
                    asrModel,
                    asrLanguage,
                    verbose,
                    reporter,
                    concurrency,
                    cancellationToken).ConfigureAwait(false);

                reporter?.MarkComplete(chapterId);
            }
            catch (OperationCanceledException oce)
            {
                reporter?.MarkFailed(chapterId, "Cancelled");
                errors.Add(new InvalidOperationException($"Pipeline cancelled for {chapter.FullName}", oce));
            }
            catch (Exception ex)
            {
                reporter?.MarkFailed(chapterId, ex.Message);
                errors.Add(new InvalidOperationException($"Pipeline failed for {chapter.FullName}: {ex.Message}", ex));
            }
            finally
            {
                workerSemaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        if (!errors.IsEmpty)
        {
            throw new AggregateException(errors);
        }

        Log.Debug("Parallel pipeline run complete.");
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
                Log.Debug("pipeline stats cancelled");
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

    private static Command CreateVerifyCommand()
    {
        var cmd = new Command("verify", "Verify treated WAVs by comparing speech activity against raw audio");

        var rootOption = new Option<DirectoryInfo?>(
            "--root",
            () => null,
            "Root directory containing raw chapter WAV files (defaults to the REPL working directory or current directory)");
        rootOption.AddAlias("-r");

        var chapterOption = new Option<string?>(
            "--chapter",
            () => null,
            "Specific chapter to verify (stem or WAV filename)");

        var allOption = new Option<bool>(
            "--all",
            () => false,
            "Verify every chapter WAV under the root directory");

        var reportDirOption = new Option<DirectoryInfo?>(
            "--report-dir",
            () => null,
            "Directory where verification reports should be written (defaults to each chapter directory)");

        var formatOption = new Option<string>(
            "--format",
            () => "json",
            "Report format: json, csv, or both");
        formatOption.FromAmong("json", "csv", "both");

        var windowOption = new Option<double>(
            "--window-ms",
            () => 30.0,
            "RMS analysis window length (ms)");

        var stepOption = new Option<double>(
            "--step-ms",
            () => 15.0,
            "RMS hop length between windows (ms)");

        var minDurationOption = new Option<double>(
            "--min-duration-ms",
            () => 60.0,
            "Minimum mismatch duration that should be reported (ms)");

        var mergeGapOption = new Option<double>(
            "--merge-gap-ms",
            () => 40.0,
            "Merge consecutive mismatches separated by at most this gap (ms)");

        cmd.AddOption(rootOption);
        cmd.AddOption(chapterOption);
        cmd.AddOption(allOption);
        cmd.AddOption(reportDirOption);
        cmd.AddOption(formatOption);
        cmd.AddOption(windowOption);
        cmd.AddOption(stepOption);
        cmd.AddOption(minDurationOption);
        cmd.AddOption(mergeGapOption);

        cmd.SetHandler(context =>
        {
            var cancellationToken = context.GetCancellationToken();
            var root = CommandInputResolver.ResolveDirectory(context.ParseResult.GetValueForOption(rootOption));
            root.Refresh();
            if (!root.Exists)
            {
                throw new DirectoryNotFoundException($"Root directory not found: {root.FullName}");
            }

            var chapter = context.ParseResult.GetValueForOption(chapterOption);
            var verifyAll = context.ParseResult.GetValueForOption(allOption);
            var reportDirValue = context.ParseResult.GetValueForOption(reportDirOption);

            DirectoryInfo? reportDir = null;
            if (reportDirValue is not null)
            {
                EnsureDirectory(reportDirValue.FullName);
                reportDirValue.Refresh();
                reportDir = reportDirValue;
            }

            var formatToken = context.ParseResult.GetValueForOption(formatOption)?.Trim().ToLowerInvariant() ?? "json";
            var format = formatToken switch
            {
                "json" => VerificationReportFormat.Json,
                "csv" => VerificationReportFormat.Csv,
                "both" => VerificationReportFormat.Both,
                _ => throw new InvalidOperationException($"Unsupported report format: {formatToken}")
            };

            var windowMs = context.ParseResult.GetValueForOption(windowOption);
            var stepMs = context.ParseResult.GetValueForOption(stepOption);
            var minDurationMs = context.ParseResult.GetValueForOption(minDurationOption);
            var mergeGapMs = context.ParseResult.GetValueForOption(mergeGapOption);

            try
            {
                RunVerify(root, reportDir, chapter, verifyAll, format, windowMs, stepMs, minDurationMs, mergeGapMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Log.Debug("pipeline verify cancelled");
                context.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "pipeline verify command failed");
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

        var adjustedOption = new Option<bool>(
            "--adjusted",
            () => false,
            "Stage pause-adjusted WAVs instead of treated WAVs.");

        cmd.AddOption(rootOption);
        cmd.AddOption(outputOption);
        cmd.AddOption(overwriteOption);
        cmd.AddOption(adjustedOption);

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
            var useAdjusted = context.ParseResult.GetValueForOption(adjustedOption);

            var destNormalized = NormalizeDirectoryPath(destination.FullName);

            var searchPattern = useAdjusted ? "*.pause-adjusted.wav" : "*.treated.wav";
            var stagedFiles = Directory.EnumerateFiles(root.FullName, searchPattern, SearchOption.AllDirectories)
                .Where(path => !IsWithinDirectory(path, destNormalized))
                .Select(path => new FileInfo(path))
                .OrderBy(file => file.FullName, PathComparer)
                .ToList();

            if (stagedFiles.Count == 0)
            {
                Log.Debug(
                    useAdjusted
                        ? "No pause-adjusted WAV files found under {Root}"
                        : "No treated WAV files found under {Root}",
                    root.FullName);
                return;
            }

            Log.Debug(
                useAdjusted
                    ? "Staging {Count} pause-adjusted file(s) from {Root} to {Destination}"
                    : "Staging {Count} treated file(s) from {Root} to {Destination}",
                stagedFiles.Count,
                root.FullName,
                destination.FullName);

            var stagedCount = 0;
            foreach (var file in stagedFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var targetName = GetStagedFileName(file);
                var targetPath = Path.Combine(destination.FullName, targetName);

                if (!overwrite && File.Exists(targetPath))
                {
                    Log.Debug("Skipping {Source}; destination already exists at {Destination} (use --overwrite to replace)", file.FullName, targetPath);
                    continue;
                }

                File.Copy(file.FullName, targetPath, overwrite);
                stagedCount++;
            }

            Log.Debug("Staged {Count} file(s) into {Destination}", stagedCount, destination.FullName);
        });

        return cmd;
    }

    private static Command CreatePrepRenameCommand()
    {
        var cmd = new Command("rename", "Rename chapter audio and related artifacts using a naming pattern.");

        var rootOption = new Option<DirectoryInfo?>
        (
            "--root",
            () => null,
            "Root directory containing chapter files (defaults to the REPL working directory or current directory)"
        );
        rootOption.AddAlias("-r");

        var patternOption = new Option<string>
        (
            "--pattern",
            description: "Naming template. Use {d}/{dd} (with optional +/- offsets) and {um#} for unmatched text segments."
        )
        {
            IsRequired = true
        };
        patternOption.AddAlias("-p");

        var dryRunOption = new Option<bool>
        (
            "--dry-run",
            () => false,
            "Preview the planned renames without touching the filesystem."
        );

        var allOption = new Option<bool>
        (
            "--all",
            () => false,
            "Force rename across every detected chapter, ignoring the REPL's active chapter scope."
        );

        cmd.AddOption(rootOption);
        cmd.AddOption(patternOption);
        cmd.AddOption(dryRunOption);
        cmd.AddOption(allOption);

        cmd.SetHandler(context =>
        {
            var rootOverride = context.ParseResult.GetValueForOption(rootOption);
            var pattern = context.ParseResult.GetValueForOption(patternOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var forceAll = context.ParseResult.GetValueForOption(allOption);

            if (string.IsNullOrWhiteSpace(pattern))
            {
                Log.Error("pipeline prep rename requires --pattern");
                context.ExitCode = 1;
                return;
            }

            DirectoryInfo root;
            try
            {
                root = CommandInputResolver.ResolveDirectory(rootOverride);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to resolve chapter root");
                context.ExitCode = 1;
                return;
            }

            root.Refresh();
            if (!root.Exists)
            {
                Log.Error("Root directory not found: {Path}", root.FullName);
                context.ExitCode = 1;
                return;
            }

            var chapters = ResolveRenameTargets(root, forceAll);
            if (chapters.Count == 0)
            {
                Log.Debug("No chapter WAV files detected under {Root}", root.FullName);
                return;
            }

            var renamePlans = new List<ChapterRenamePlan>();
            var newStemSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                var oldStem = Path.GetFileNameWithoutExtension(chapter.Name);
                var unmatchedParts = ExtractUnmatchedParts(oldStem);
                var newStem = ApplyRenamePattern(pattern, i, unmatchedParts).Trim();

                if (string.IsNullOrEmpty(newStem))
                {
                    Log.Error("Pattern produced an empty name for chapter {Stem}", oldStem);
                    context.ExitCode = 1;
                    return;
                }

                if (newStem.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    Log.Error("Pattern produced an invalid file name '{Name}'", newStem);
                    context.ExitCode = 1;
                    return;
                }

                if (!newStemSet.Add(newStem))
                {
                    Log.Error("Pattern would produce duplicate name '{Name}'", newStem);
                    context.ExitCode = 1;
                    return;
                }

                if (string.Equals(oldStem, newStem, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Debug("Chapter {Stem} already matches the requested pattern; skipping.", oldStem);
                    continue;
                }

                renamePlans.Add(BuildRenamePlan(chapter, oldStem, newStem));
            }

            if (renamePlans.Count == 0)
            {
                Log.Debug("No renames required.");
                return;
            }

            try
            {
                ValidateRenamePlans(renamePlans);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                context.ExitCode = 1;
                return;
            }

            foreach (var plan in renamePlans)
            {
                Log.Debug("Renaming {Old} -> {New}", plan.OldStem, plan.NewStem);
                if (dryRun)
                {
                    foreach (var op in plan.FileOps)
                    {
                        if (!PathsEqual(op.Source, op.Target))
                        {
                            Log.Debug("  FILE: {Source} => {Target}", op.Source, op.Target);
                        }
                    }

                    foreach (var op in plan.DirectoryOps)
                    {
                        if (!PathsEqual(op.Source, op.Target))
                        {
                            Log.Debug("  DIR : {Source} => {Target}", op.Source, op.Target);
                        }
                    }
                }
            }

            if (dryRun)
            {
                Log.Debug("Dry run only; no files were renamed.");
                return;
            }

            foreach (var plan in renamePlans)
            {
                foreach (var op in plan.FileOps)
                {
                    if (PathsEqual(op.Source, op.Target))
                    {
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(op.Target)!);
                    File.Move(op.Source, op.Target);
                }
            }

            foreach (var plan in renamePlans)
            {
                foreach (var op in plan.DirectoryOps.OrderByDescending(d => d.Source.Length))
                {
                    if (PathsEqual(op.Source, op.Target))
                    {
                        continue;
                    }

                    Directory.Move(op.Source, op.Target);
                }
            }

            Log.Debug("Renamed {Count} chapter(s)", renamePlans.Count);

            var repl = ReplContext.Current;
            if (repl is not null && PathsEqual(repl.WorkingDirectory, root.FullName))
            {
                repl.RefreshChapters();
            }
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
                Log.Debug("Hard reset: deleting all generated content under {Root} (DOCX files preserved)", root.FullName);
                PerformHardReset(root, cancellationToken);
            }
            else
            {
                Log.Debug("Resetting chapter directories under {Root}", root.FullName);
                PerformSoftReset(root, cancellationToken);
            }
        });

        return cmd;
    }

    private static Command CreateRun(PipelineService pipelineService)
    {
        ArgumentNullException.ThrowIfNull(pipelineService);

        var cmd = new Command("run", "Build index, run ASR, align, and hydrate in one pass");

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

        var verboseOption = new Option<bool>("--verbose", () => false, "Enable verbose logging for pipeline stages.");
        var maxWorkersOption = new Option<int>("--max-workers", () => Math.Max(1, Environment.ProcessorCount), "Maximum number of chapters to process in parallel once ASR is complete");
        var maxMfaOption = new Option<int>("--max-mfa", () => Math.Max(1, Environment.ProcessorCount / 2), "Maximum number of concurrent MFA alignment jobs");
        var progressOption = new Option<bool>("--progress", () => true, "Display live progress UI while running the pipeline");

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
        cmd.AddOption(verboseOption);
        cmd.AddOption(maxWorkersOption);
        cmd.AddOption(maxMfaOption);
        cmd.AddOption(progressOption);

        cmd.SetHandler(async context =>
        {
            var cancellationToken = context.GetCancellationToken();
            var bookFile = CommandInputResolver.ResolveBookSource(context.ParseResult.GetValueForOption(bookOption));
            var workDir = context.ParseResult.GetValueForOption(workDirOption);
            var bookIndex = context.ParseResult.GetValueForOption(bookIndexOption) ?? CommandInputResolver.ResolveBookIndex(null, mustExist: false);
            var forceAll = context.ParseResult.GetValueForOption(forceOption);
            var forceIndex = context.ParseResult.GetValueForOption(forceIndexOption);
            var avgWpm = context.ParseResult.GetValueForOption(avgWpmOption);
            var asrServiceUrl = context.ParseResult.GetValueForOption(asrServiceOption) ?? "http://localhost:8000";
            var asrModel = context.ParseResult.GetValueForOption(asrModelOption);
            var asrLanguage = context.ParseResult.GetValueForOption(asrLanguageOption) ?? "en";
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var maxWorkers = context.ParseResult.GetValueForOption(maxWorkersOption);
            var maxMfa = context.ParseResult.GetValueForOption(maxMfaOption);
            var showProgress = context.ParseResult.GetValueForOption(progressOption);
            if (showProgress && Log.IsDebugLoggingEnabled())
            {
                Log.Debug("Progress UI disabled while AMS_LOG_LEVEL requests Debug-level logging.");
                showProgress = false;
            }

            var repl = ReplContext.Current;
            if (repl?.RunAllChapters == true && repl.Chapters.Count > 0)
            {
                try
                {
                    if (showProgress)
                    {
                        await AnsiConsole.Progress()
                            .AutoClear(false)
                            .Columns(PipelineProgressColumns)
                            .StartAsync(async progressContext =>
                            {
                                await RunPipelineForMultipleChaptersAsync(
                                    pipelineService,
                                    bookFile,
                                    workDir,
                                    bookIndex,
                                    forceIndex,
                                    forceAll,
                                    avgWpm,
                                    asrServiceUrl,
                                    asrModel,
                                    asrLanguage,
                                    verbose,
                                    repl.Chapters,
                                    maxWorkers,
                                    maxMfa,
                                    progressContext,
                                    cancellationToken).ConfigureAwait(false);
                            }).ConfigureAwait(false);
                    }
                    else
                    {
                        await RunPipelineForMultipleChaptersAsync(
                            pipelineService,
                            bookFile,
                            workDir,
                            bookIndex,
                            forceIndex,
                            forceAll,
                            avgWpm,
                            asrServiceUrl,
                            asrModel,
                            asrLanguage,
                            verbose,
                            repl.Chapters,
                            maxWorkers,
                            maxMfa,
                            progressContext: null,
                            cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "pipeline run command failed");
                    context.ExitCode = 1;
                }
                return;
            }

            var audioFile = CommandInputResolver.RequireAudio(context.ParseResult.GetValueForOption(audioOption));
            var chapterId = context.ParseResult.GetValueForOption(chapterIdOption) ?? Path.GetFileNameWithoutExtension(audioFile.Name);

            try
            {
                if (showProgress)
                {
                    await AnsiConsole.Progress()
                        .AutoClear(false)
                        .Columns(PipelineProgressColumns)
                        .StartAsync(async progressContext =>
                        {
                            var reporter = new PipelineProgressReporter(progressContext, new[] { audioFile });
                            reporter.SetQueued(chapterId);

                            using var concurrency = PipelineConcurrencyControl.CreateSingle();

                            try
                            {
                                await RunPipelineAsync(
                                    pipelineService,
                                    bookFile,
                                    audioFile,
                                    workDir,
                                    bookIndex,
                                    chapterId,
                                    forceIndex,
                                    forceAll,
                                    avgWpm,
                                    asrServiceUrl,
                                    asrModel,
                                    asrLanguage,
                                    verbose,
                                    reporter,
                                    concurrency,
                                    cancellationToken).ConfigureAwait(false);

                                reporter.MarkComplete(chapterId);
                            }
                            catch (OperationCanceledException)
                            {
                                reporter.MarkFailed(chapterId, "Cancelled");
                                throw;
                            }
                            catch (Exception ex)
                            {
                                reporter.MarkFailed(chapterId, ex.Message);
                                throw;
                            }
                        }).ConfigureAwait(false);
                }
                else
                {
                    using var concurrency = PipelineConcurrencyControl.CreateSingle();
                    await RunPipelineAsync(
                        pipelineService,
                        bookFile,
                        audioFile,
                        workDir,
                        bookIndex,
                        chapterId,
                        forceIndex,
                        forceAll,
                        avgWpm,
                        asrServiceUrl,
                        asrModel,
                        asrLanguage,
                        verbose,
                        progress: null,
                        concurrency,
                        cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                context.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "pipeline run command failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

private static async Task RunPipelineAsync(
    PipelineService pipelineService,
        FileInfo bookFile,
        FileInfo audioFile,
        DirectoryInfo? workDirOption,
        FileInfo? bookIndexOverride,
        string? chapterIdOverride,
        bool forceIndex,
        bool force,
        double avgWpm,
        string asrServiceUrl,
        string? asrModel,
        string asrLanguage,
        bool verbose,
        PipelineProgressReporter? progress,
        PipelineConcurrencyControl concurrency,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pipelineService);

        bool quiet = progress is not null;
        bool logInfo = !quiet || verbose;
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

        LogStageInfo(logInfo, "=== AMS Pipeline ===");
        LogStageInfo(logInfo, "Book={BookFile}", bookFile.FullName);
        LogStageInfo(logInfo, "Audio={AudioFile}", audioFile.FullName);
        LogStageInfo(logInfo, "WorkDir={WorkDir}", workDirPath);
        LogStageInfo(logInfo, "Chapter={Chapter}", chapterStem);
        LogStageInfo(logInfo, "ChapterDir={ChapterDir}", chapterDir);

        var defaultAnchorOptions = new AnchorComputationOptions
        {
            DetectSection = true,
            AsrPrefixTokens = 8,
            NGram = 3,
            TargetPerTokens = 50,
            MinSeparation = 100,
            AllowBoundaryCross = false,
            UseDomainStopwords = true
        };

        var transcriptOptions = new GenerateTranscriptOptions
        {
            Engine = AsrEngineConfig.Resolve(),
            ServiceUrl = asrServiceUrl,
            Model = asrModel,
            Language = asrLanguage
        };

        var pipelineOptions = new PipelineRunOptions
        {
            BookFile = bookFile,
            BookIndexFile = bookIndexFile,
            AudioFile = audioFile,
            ChapterDirectory = chapterDirInfo,
            ChapterId = chapterStem,
            Force = force,
            ForceIndex = forceIndex,
            AverageWordsPerMinute = avgWpm,
            TranscriptOptions = transcriptOptions,
            AnchorOptions = defaultAnchorOptions with { EmitWindows = false },
            TranscriptIndexOptions = new BuildTranscriptIndexOptions
            {
                AudioFile = audioFile,
                AsrFile = asrFile,
                BookIndexFile = bookIndexFile,
                AnchorOptions = defaultAnchorOptions with { EmitWindows = true }
            },
            HydrationOptions = null,
            MfaOptions = new RunMfaOptions
            {
                AudioFile = audioFile,
                HydrateFile = hydrateFile,
                TextGridFile = textGridFile,
                AlignmentRootDirectory = new DirectoryInfo(Path.Combine(chapterDir, "alignment"))
            },
            MergeOptions = new MergeTimingsOptions
            {
                HydrateFile = hydrateFile,
                TranscriptFile = txFile,
                TextGridFile = textGridFile
            },
            TreatedCopyFile = treatedWav,
            Concurrency = concurrency
        };

        var result = await pipelineService.RunChapterAsync(pipelineOptions, cancellationToken).ConfigureAwait(false);

        var bookIndexMessage = result.BookIndexBuilt ? "Index built" : "Index ready";
        progress?.ReportStage(chapterStem, PipelineStage.BookIndex, bookIndexMessage);
        progress?.ReportStage(chapterStem, PipelineStage.Asr, result.AsrRan ? "ASR complete" : "ASR cached");
        progress?.ReportStage(chapterStem, PipelineStage.Anchors, result.AnchorsRan ? "Anchors generated" : "Anchors cached");
        progress?.ReportStage(chapterStem, PipelineStage.Transcript, result.TranscriptRan ? "Transcript indexed" : "Transcript cached");
        progress?.ReportStage(chapterStem, PipelineStage.Hydrate, result.HydrateRan ? "Hydrate complete" : "Hydrate cached");

        var textGridStatus = result.TextGridFile.Exists
            ? (result.MfaRan ? "MFA aligned" : "MFA cached")
            : "MFA missing";
        progress?.ReportStage(chapterStem, PipelineStage.Mfa, textGridStatus);

        LogStageInfo(logInfo, "Book index : {Status}", result.BookIndexBuilt ? "Rebuilt" : "Cached");
        LogStageInfo(logInfo, "ASR        : {Status}", result.AsrRan ? "Executed" : "Cached");
        LogStageInfo(logInfo, "Anchors    : {Status}", result.AnchorsRan ? "Executed" : "Cached");
        LogStageInfo(logInfo, "Transcript : {Status}", result.TranscriptRan ? "Executed" : "Cached");
        LogStageInfo(logInfo, "Hydrate    : {Status}", result.HydrateRan ? "Executed" : "Cached");
        LogStageInfo(logInfo, "MFA        : {Status}", textGridStatus);

        bookIndexFile = result.BookIndexFile;
        asrFile = result.AsrFile;
        anchorsFile = result.AnchorFile;
        txFile = result.TranscriptFile;
        hydrateFile = result.HydrateFile;
        textGridFile = result.TextGridFile;
        treatedWav = result.TreatedAudioFile;

        LogStageInfo(logInfo, "=== Outputs ===");
        LogStageInfo(logInfo, "Book index : {BookIndex}", bookIndexFile.FullName);
        LogStageInfo(logInfo, "ASR JSON   : {AsrFile}", asrFile.FullName);
        LogStageInfo(logInfo, "Anchors    : {AnchorsFile}", anchorsFile.FullName);
        LogStageInfo(logInfo, "Transcript : {TranscriptFile}", txFile.FullName);
        LogStageInfo(logInfo, "Hydrated   : {HydratedFile}", hydrateFile.FullName);
        LogStageInfo(logInfo, "Treated    : {TreatedFile}", treatedWav.FullName);
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
                Log.Debug("Deleted chapter directory {Directory}", directory.FullName);
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to delete {Directory}: {Message}", directory.FullName, ex.Message);
            }
        }

        Log.Debug("Soft reset complete. Removed {Count} chapter directorie(s).", deleted);
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
                Log.Debug("Deleted directory {Directory}", directory.FullName);
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to delete {Directory}: {Message}", directory.FullName, ex.Message);
            }
        }

        // Delete files except DOCX
        foreach (var file in root.EnumerateFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) || file.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                file.Delete();
                Log.Debug("Deleted file {File}", file.FullName);
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to delete {File}: {Message}", file.FullName, ex.Message);
            }
        }

        Log.Debug("Hard reset complete for {Root}", root.FullName);
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
                Log.Debug("Book index not found at {Path}", bookIndexFile.FullName);
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
                Log.Debug("Failed to load book index {Path}: {Message}", bookIndexFile.FullName, ex.Message);
                bookIndexFile = null;
            }
        }

        if (bookIndex is null)
        {
            Log.Debug("Book index unavailable; prosody statistics will be skipped.");
        }

        var chapterDirs = ResolveChapterDirectories(root, chapterName, analyzeAll);
        if (chapterDirs.Count == 0)
        {
            Log.Debug("No chapter directories found under {Root}", root.FullName);
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
                Log.Debug("Failed to compute stats for {Chapter}: {Message}", chapterDir.Name, ex.Message);
            }
        }

        if (statsList.Count == 0)
        {
            Log.Debug("No statistics could be generated.");
            return;
        }

        PrintStatsReport(root, bookIndexFile, bookIndex, statsList, totalAudioSec);
    }

    private static List<FileInfo> ResolveRenameTargets(DirectoryInfo root, bool forceAll = false)
    {
        if (!forceAll)
        {
            var repl = ReplContext.Current;
            if (repl is not null && PathsEqual(repl.WorkingDirectory, root.FullName))
            {
                if (repl.RunAllChapters)
                {
                    if (repl.ActiveChapter is not null)
                    {
                        return new List<FileInfo> { repl.ActiveChapter };
                    }

                    return repl.Chapters.ToList();
                }

                if (repl.ActiveChapter is not null)
                {
                    return new List<FileInfo> { repl.ActiveChapter };
                }

                return new List<FileInfo>();
            }
        }

        return Directory.EnumerateFiles(root.FullName, "*.wav", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ApplyRenamePattern(string pattern, int index, IReadOnlyList<string> unmatchedParts)
    {
        var result = PatternTokenRegex.Replace(pattern, match =>
        {
            var token = match.Groups[1].Value.ToLowerInvariant();
            var offsetGroup = match.Groups[2];
            if (token.StartsWith("um", StringComparison.Ordinal))
            {
                if (offsetGroup.Success)
                {
                    throw new InvalidOperationException($"Token '{match.Value}' does not support +/- offsets.");
                }

                if (token.Equals("um*", StringComparison.Ordinal))
                {
                    return unmatchedParts.Count == 0
                        ? string.Empty
                        : string.Join("_", unmatchedParts);
                }

                if (token.Contains('-'))
                {
                    var rangeParts = token[2..].Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (rangeParts.Length != 2 ||
                        !int.TryParse(rangeParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var startIndex) ||
                        !int.TryParse(rangeParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var endIndex) ||
                        startIndex <= 0 || endIndex < startIndex)
                    {
                        throw new InvalidOperationException($"Invalid unmatched range token '{match.Value}'.");
                    }

                    int startZero = startIndex - 1;
                    int endZero = Math.Min(endIndex - 1, unmatchedParts.Count - 1);
                    if (startZero >= unmatchedParts.Count || startZero > endZero)
                    {
                        return string.Empty;
                    }

                    var slice = unmatchedParts
                        .Skip(startZero)
                        .Take(endZero - startZero + 1);
                    return string.Join("_", slice);
                }
                else
                {
                    if (!int.TryParse(token.AsSpan(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var partIndex) || partIndex <= 0)
                    {
                        throw new InvalidOperationException($"Invalid unmatched token '{match.Value}'.");
                    }

                    var zeroIndex = partIndex - 1;
                    return zeroIndex >= 0 && zeroIndex < unmatchedParts.Count
                        ? unmatchedParts[zeroIndex]
                        : string.Empty;
                }
            }
            else
            {
                var offset = offsetGroup.Success ? int.Parse(offsetGroup.Value, CultureInfo.InvariantCulture) : 0;
                var value = index + offset;
                return token.Length > 1
                    ? value.ToString("D" + token.Length, CultureInfo.InvariantCulture)
                    : value.ToString(CultureInfo.InvariantCulture);
            }
        });

        if (result.IndexOf('{') >= 0 || result.IndexOf('}') >= 0)
        {
            throw new InvalidOperationException($"Pattern contains unsupported token: {pattern}");
        }

        return result;
    }

    private static readonly Regex UnmatchedTokenRegex = new(@"[A-Za-z]+[A-Za-z0-9]*", RegexOptions.Compiled);

    private static string[] ExtractUnmatchedParts(string stem)
    {
        if (string.IsNullOrWhiteSpace(stem))
        {
            return Array.Empty<string>();
        }

        var matches = UnmatchedTokenRegex.Matches(stem);
        if (matches.Count == 0)
        {
            return Array.Empty<string>();
        }

        var parts = new string[matches.Count];
        for (int i = 0; i < matches.Count; i++)
        {
            parts[i] = matches[i].Value;
        }
        return parts;
    }

    private static ChapterRenamePlan BuildRenamePlan(FileInfo chapter, string oldStem, string newStem)
    {
        var root = chapter.Directory?.FullName
                   ?? throw new InvalidOperationException($"Cannot resolve directory for {chapter.FullName}");

        var directoryOps = new List<RenameOp>();
        var fileOps = new List<RenameOp>();
        CollectRenameOperations(root, root, root, oldStem, newStem, directoryOps, fileOps);

        return new ChapterRenamePlan(chapter, oldStem, newStem, directoryOps, fileOps);
    }

    private static void CollectRenameOperations(
        string root,
        string currentDirOldPath,
        string currentDirNewPath,
        string oldStem,
        string newStem,
        List<RenameOp> directoryOps,
        List<RenameOp> fileOps)
    {
        foreach (var subDir in Directory.EnumerateDirectories(currentDirOldPath))
        {
            var dirName = Path.GetFileName(subDir)!;
            var newDirName = ReplaceStem(dirName, oldStem, newStem);
            var targetDir = Path.Combine(currentDirNewPath, newDirName);

            if (!PathsEqual(subDir, targetDir))
            {
                directoryOps.Add(new RenameOp(subDir, targetDir));
            }

            CollectRenameOperations(root, subDir, targetDir, oldStem, newStem, directoryOps, fileOps);
        }

        foreach (var filePath in Directory.EnumerateFiles(currentDirOldPath))
        {
            var fileName = Path.GetFileName(filePath)!;
            var newFileName = ReplaceStem(fileName, oldStem, newStem);
            var targetFile = Path.Combine(currentDirOldPath, newFileName);

            if (!PathsEqual(filePath, targetFile))
            {
                fileOps.Add(new RenameOp(filePath, targetFile));
            }
        }
    }

    private static void ValidateRenamePlans(IEnumerable<ChapterRenamePlan> plans)
    {
        var dirOps = plans.SelectMany(p => p.DirectoryOps).ToList();
        var fileOps = plans.SelectMany(p => p.FileOps).ToList();

        var dirTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var op in dirOps)
        {
            if (PathsEqual(op.Source, op.Target))
            {
                continue;
            }

            if ((Directory.Exists(op.Target) || File.Exists(op.Target)) && !PathsEqual(op.Source, op.Target))
            {
                throw new InvalidOperationException($"Target already exists: {op.Target}");
            }

            dirTargets.Add(Path.GetFullPath(op.Target));
        }

        var orderedDirOps = dirOps.OrderByDescending(op => op.Source.Length).ToList();
        var fileTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var op in fileOps)
        {
            if (PathsEqual(op.Source, op.Target))
            {
                continue;
            }

            if (File.Exists(op.Target) && !PathsEqual(op.Source, op.Target))
            {
                throw new InvalidOperationException($"Target already exists: {op.Target}");
            }

            var finalPath = ProjectPath(op.Target, orderedDirOps);

            if (!fileTargets.Add(Path.GetFullPath(finalPath)))
            {
                throw new InvalidOperationException($"Multiple items would be renamed to {finalPath}");
            }
        }
    }

    private static string ProjectPath(string path, IReadOnlyList<RenameOp> directoryOps)
    {
        var projected = path;
        foreach (var op in directoryOps)
        {
            var source = EnsureTrailingSeparator(op.Source);
            if (projected.StartsWith(source, PathComparison))
            {
                projected = EnsureTrailingSeparator(op.Target) + projected[source.Length..];
                continue;
            }

            if (PathsEqual(projected, op.Source))
            {
                projected = op.Target;
            }
        }

        return projected;
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }

    private static string ReplaceStem(string name, string oldStem, string newStem)
    {
        return name.StartsWith(oldStem, StringComparison.OrdinalIgnoreCase)
            ? newStem + name[oldStem.Length..]
            : name;
    }

    private static bool PathsEqual(string? left, string? right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), PathComparison);
    }

    private sealed record RenameOp(string Source, string Target);

    private sealed record ChapterRenamePlan(
        FileInfo ChapterFile,
        string OldStem,
        string NewStem,
        List<RenameOp> DirectoryOps,
        List<RenameOp> FileOps);

    private static List<DirectoryInfo> ResolveChapterDirectories(DirectoryInfo root, string? chapterName, bool analyzeAll)
    {
        var chapters = new List<DirectoryInfo>();

        if (!string.IsNullOrWhiteSpace(chapterName))
        {
            var explicitDir = new DirectoryInfo(Path.Combine(root.FullName, chapterName));
            if (!explicitDir.Exists)
            {
                Log.Debug("Chapter directory not found: {Directory}", explicitDir.FullName);
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

        Log.Debug("Multiple chapter directories detected under {Root}. Use --chapter <name> or --all.", root.FullName);
        return chapters;
    }

    private static ChapterStats? ComputeChapterStats(DirectoryInfo chapterDir, BookIndex? bookIndex, FileInfo? bookIndexFile)
    {
        var txFile = chapterDir.EnumerateFiles("*.align.tx.json", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (txFile is null)
        {
            Log.Debug("Skipping {Chapter}: transcript index not found", chapterDir.FullName);
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
            Log.Debug("Skipping {Chapter}: treated audio not found", chapterDir.FullName);
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
                var (policy, policyPath) = PausePolicyResolver.Resolve(txFile);
                if (!string.IsNullOrWhiteSpace(policyPath))
                {
                    Log.Debug("Pause policy loaded for {Chapter} from {Path}", chapterDir.Name, policyPath);
                }
                var pauseMap = PauseMapBuilder.Build(transcript, bookIndex, hydrated, policy, silences, includeAllIntraSentenceGaps: true);
                prosodyStats = pauseMap.Stats;
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to compute prosody stats for {Chapter}: {Message}", chapterDir.FullName, ex.Message);
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

    private static void RunVerify(
        DirectoryInfo root,
        DirectoryInfo? reportDir,
        string? chapterName,
        bool verifyAll,
        VerificationReportFormat format,
        double windowMs,
        double stepMs,
        double minDurationMs,
        double mergeGapMs,
        CancellationToken cancellationToken)
    {
        var rawTargets = ResolveVerifyTargets(root, chapterName, verifyAll);
        if (rawTargets.Count == 0)
        {
            Log.Debug("No chapter WAV files found under {Root}", root.FullName);
            return;
        }

        int processed = 0;
        int withMismatches = 0;
        int skipped = 0;

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Pipeline Verify[/]")
            .AddColumn("Variant")
            .AddColumn("Status")
            .AddColumn("Missing (s)")
            .AddColumn("Extra (s)")
            .AddColumn("Mismatches")
            .AddColumn("Reports");

        AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
            {
                foreach (var raw in rawTargets)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var stem = Path.GetFileNameWithoutExtension(raw.Name);
                    var chapterDir = new DirectoryInfo(Path.Combine(root.FullName, stem));
                    chapterDir.Refresh();

                    try
                    {
                        var hydrateFile = new FileInfo(Path.Combine(chapterDir.FullName, $"{stem}.align.hydrate.json"));
                        string? referenceHydratePath = null;
                        if (hydrateFile.Exists)
                        {
                            referenceHydratePath = hydrateFile.FullName;
                        }

                        if (referenceHydratePath is null)
                        {
                            skipped++;
                            table.AddRow(
                                Markup.Escape(stem),
                                "[yellow]Skipped (no hydrate)[/]",
                                "-",
                                "-",
                                "-",
                                "[grey]-[/]");
                            ctx.Refresh();
                            Log.Debug("Skipping {Chapter}: hydrate JSON not found", stem);
                            continue;
                        }

                        var variants = ResolveProcessedVariants(root, chapterDir, stem, referenceHydratePath);
                        if (variants.Count == 0)
                        {
                            skipped++;
                            var status = "[yellow]Skipped (no processed WAV)[/]";
                            table.AddRow(
                                Markup.Escape(stem),
                                status,
                                "-",
                                "-",
                                "-",
                                "[grey]-[/]");
                            ctx.Refresh();
                            Log.Debug("Skipping {Chapter}: no treated or pause-adjusted WAV found", stem);
                            continue;
                        }

                        float[] rawMono;
                        int rawSampleRate;
                        IReadOnlyDictionary<int, AudioSentenceTiming> rawTimings;
                        try
                        {
                            var rawBuffer = AudioProcessor.Decode(raw.FullName);
                            rawSampleRate = rawBuffer.SampleRate;
                            rawMono = ToMono(rawBuffer);
                            rawTimings = LoadSentenceTimings(referenceHydratePath);
                        }
                        catch (Exception ex)
                        {
                            skipped++;
                            table.AddRow(
                                Markup.Escape(stem),
                                "[red]Error[/]",
                                "-",
                                "-",
                                "-",
                                "[grey]-[/]");
                            ctx.Refresh();
                            Log.Error(ex, "Failed to load raw assets for {Chapter}", stem);
                            continue;
                        }

                        var chapterLabel = stem;

                        foreach (var variant in variants)
                        {
                            var rowLabel = variants.Count > 1
                                ? $"{stem} ({variant.Variant})"
                                : stem;

                            AudioVerificationResult result;
                            try
                            {
                                var variantBuffer = AudioProcessor.Decode(variant.File.FullName);
                                var variantMono = ToMono(variantBuffer);
                                var variantTimings = string.Equals(variant.HydratePath, referenceHydratePath, StringComparison.OrdinalIgnoreCase)
                                    ? rawTimings
                                    : LoadSentenceTimings(variant.HydratePath);

                                result = AudioIntegrityVerifier.Verify(
                                    rawMono,
                                    rawSampleRate,
                                    variantMono,
                                    variantBuffer.SampleRate,
                                    rawTimings,
                                    variantTimings,
                                    windowMs: windowMs,
                                    stepMs: stepMs,
                                    minMismatchMs: minDurationMs,
                                    minGapToMergeMs: mergeGapMs);
                            }
                            catch (Exception ex)
                            {
                                skipped++;
                                table.AddRow(
                                    Markup.Escape(rowLabel),
                                    "[red]Error[/]",
                                    "-",
                                    "-",
                                    "-",
                                    "[grey]-[/]");
                                ctx.Refresh();
                                Log.Error(ex, "Verification failed for {Chapter} ({Variant})", stem, variant.Variant);
                                continue;
                            }

                            var outputDir = reportDir ?? chapterDir;
                            EnsureDirectory(outputDir.FullName);

                            var artifacts = new List<string>();
                            var variantToken = NormalizeVariantToken(variant.Variant);

                            if (format is VerificationReportFormat.Json or VerificationReportFormat.Both)
                            {
                                var jsonPath = Path.Combine(outputDir.FullName, $"{stem}.verify.{variantToken}.json");
                                using (var stream = File.Create(jsonPath))
                                {
                                    var payload = new { Chapter = stem, Variant = variant.Variant, Result = result };
                                    JsonSerializer.Serialize(stream, payload, VerifyJsonOptions);
                                }

                                artifacts.Add(jsonPath);
                            }

                            if (format is VerificationReportFormat.Csv or VerificationReportFormat.Both)
                            {
                                var csvPath = Path.Combine(outputDir.FullName, $"{stem}.verify.{variantToken}.csv");
                                WriteVerificationCsv(csvPath, chapterLabel, variant.Variant, result);
                                artifacts.Add(csvPath);
                            }

                            bool hasIssues = result.Mismatches.Count > 0;
                            var statusMarkup = hasIssues
                                ? "[red]Issues[/]"
                                : "[green]OK[/]";

                            var missingText = result.MissingSpeechSec.ToString("F3", CultureInfo.InvariantCulture);
                            var extraText = result.ExtraSpeechSec.ToString("F3", CultureInfo.InvariantCulture);
                            var mismatchText = result.Mismatches.Count.ToString(CultureInfo.InvariantCulture);
                            var reportText = artifacts.Count == 0
                                ? "[grey]-[/]"
                                : string.Join("\n", artifacts.Select(path => $"[grey]{Markup.Escape(Path.GetFileName(path))}[/]"));

                            table.AddRow(
                                Markup.Escape(rowLabel),
                                statusMarkup,
                                missingText,
                                extraText,
                                mismatchText,
                                reportText);
                            ctx.Refresh();

                            Log.Debug(
                                "Verified {Chapter} ({Variant}): mismatches={Count} missing={Missing:F3}s extra={Extra:F3}s. Reports: {Reports}",
                                stem,
                                variant.Variant,
                                result.Mismatches.Count,
                                result.MissingSpeechSec,
                                result.ExtraSpeechSec,
                                artifacts.ToArray());

                        processed++;
                        if (hasIssues)
                        {
                            withMismatches++;
                        }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Verification failed for {Chapter}", stem);
                        table.AddRow(
                            Markup.Escape(stem),
                            "[red]Error[/]",
                            "-",
                            "-",
                            "-",
                            "-",
                            "[grey]-[/]");
                        ctx.Refresh();
                    }
                }
            });

        Log.Debug(
            "Verification complete. Processed {Processed} comparison(s); {WithIssues} with issues; {Skipped} skipped.",
            processed,
            withMismatches,
            skipped);

        AnsiConsole.MarkupLine(
            "[bold]Verification Summary:[/] processed={0}, issues={1}, skipped={2}",
            processed.ToString(CultureInfo.InvariantCulture),
            withMismatches.ToString(CultureInfo.InvariantCulture),
            skipped.ToString(CultureInfo.InvariantCulture));
    }

    private static List<FileInfo> ResolveVerifyTargets(DirectoryInfo root, string? chapterName, bool verifyAll)
    {
        if (!string.IsNullOrWhiteSpace(chapterName))
        {
            var candidateName = chapterName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                ? chapterName
                : chapterName + ".wav";
            var explicitPath = Path.Combine(root.FullName, candidateName);
            if (!File.Exists(explicitPath))
            {
                Log.Debug("Chapter WAV not found: {Path}", explicitPath);
                return new List<FileInfo>();
            }

            return new List<FileInfo> { new FileInfo(explicitPath) };
        }

        var repl = ReplContext.Current;
        if (repl is not null && PathsEqual(repl.WorkingDirectory, root.FullName))
        {
            if (verifyAll || repl.RunAllChapters)
            {
                return repl.Chapters.ToList();
            }

            if (repl.ActiveChapter is not null)
            {
                return new List<FileInfo> { repl.ActiveChapter };
            }

            return new List<FileInfo>();
        }

        var allChapters = Directory.EnumerateFiles(root.FullName, "*.wav", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.Name, PathComparer)
            .ToList();

        if (verifyAll)
        {
            return allChapters;
        }

        return allChapters.Count > 0 ? new List<FileInfo> { allChapters[0] } : new List<FileInfo>();
    }

    private sealed record ProcessedVariant(string Variant, FileInfo File, bool IsPauseAdjusted, string HydratePath);

    private static IReadOnlyList<ProcessedVariant> ResolveProcessedVariants(DirectoryInfo root, DirectoryInfo chapterDir, string stem, string referenceHydratePath)
    {
        var variants = new List<ProcessedVariant>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddVariant(string variant, FileInfo file, bool isPauseAdjusted, string hydratePath)
        {
            if (!file.Exists)
            {
                return;
            }

            var fullPath = file.FullName;
            if (!seenPaths.Add(fullPath))
            {
                return;
            }

            var effectiveHydrate = !string.IsNullOrEmpty(hydratePath) && File.Exists(hydratePath)
                ? hydratePath
                : referenceHydratePath;

            variants.Add(new ProcessedVariant(variant, file, isPauseAdjusted, effectiveHydrate!));
        }

        void TryAddVariant(string variant, bool isPauseAdjusted, IEnumerable<string> candidatePaths, Func<string, string?> hydrateResolver)
        {
            foreach (var path in candidatePaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                try
                {
                    var file = new FileInfo(path);
                    var hydrate = hydrateResolver(path) ?? referenceHydratePath;
                    AddVariant(variant, file, isPauseAdjusted, hydrate);
                    if (variants.Any(v => v.Variant == variant))
                    {
                        break;
                    }
                }
                catch
                {
                    // ignore invalid paths
                }
            }
        }

        string treatedFileName = $"{stem}.treated.wav";
        TryAddVariant("treated", false, new[]
        {
            Path.Combine(chapterDir.FullName, treatedFileName),
            Path.Combine(root.FullName, treatedFileName)
        }, _ => referenceHydratePath);

        if (!variants.Any(v => v.Variant == "treated") && Directory.Exists(chapterDir.FullName))
        {
            var extraTreated = Directory.EnumerateFiles(chapterDir.FullName, "*.treated.wav", SearchOption.AllDirectories);
            TryAddVariant("treated", false, extraTreated, _ => referenceHydratePath);
        }

        string adjustedFileName = $"{stem}.pause-adjusted.wav";
        TryAddVariant("pause-adjusted", true, new[]
        {
            Path.Combine(chapterDir.FullName, adjustedFileName),
            Path.Combine(root.FullName, adjustedFileName)
        }, _ => ResolveVariantHydrate(chapterDir, stem) ?? referenceHydratePath);

        if (!variants.Any(v => v.Variant == "pause-adjusted") && Directory.Exists(chapterDir.FullName))
        {
            var extraAdjusted = Directory.EnumerateFiles(chapterDir.FullName, "*.pause-adjusted.wav", SearchOption.AllDirectories);
            TryAddVariant("pause-adjusted", true, extraAdjusted, _ => ResolveVariantHydrate(chapterDir, stem) ?? referenceHydratePath);
        }

        return variants;
    }

    private static string? ResolveVariantHydrate(DirectoryInfo chapterDir, string stem)
    {
        if (!chapterDir.Exists)
        {
            return null;
        }

        var candidates = new[]
        {
            Path.Combine(chapterDir.FullName, $"{stem}.pause-adjusted.hydrate.json"),
            Path.Combine(chapterDir.FullName, $"{stem}.pause-adjusted.align.hydrate.json"),
            Path.Combine(chapterDir.FullName, $"{stem}.pause-adjusted.timeline.hydrate.json")
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Fallback: look for any pause-adjusted hydrate under the chapter directory
        var match = Directory.EnumerateFiles(chapterDir.FullName, "*.pause-adjusted.hydrate.json", SearchOption.AllDirectories)
            .FirstOrDefault();
        return match;
    }

    private static string NormalizeVariantToken(string variant)
    {
        if (string.IsNullOrWhiteSpace(variant))
        {
            return "variant";
        }

        var chars = variant.ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        var token = new string(chars).Trim('-');
        return string.IsNullOrEmpty(token) ? "variant" : token;
    }

    private static float[] ToMono(AudioBuffer buffer)
    {
        if (buffer.Channels == 1)
        {
            return (float[])buffer.Planar[0].Clone();
        }

        var mono = new float[buffer.Length];
        float scale = 1f / buffer.Channels;
        for (int ch = 0; ch < buffer.Channels; ch++)
        {
            var src = buffer.Planar[ch];
            for (int i = 0; i < mono.Length; i++)
            {
                mono[i] += src[i] * scale;
            }
        }

        return mono;
    }

    private static IReadOnlyDictionary<int, AudioSentenceTiming> LoadSentenceTimings(string hydratePath)
    {
        using var stream = File.OpenRead(hydratePath);
        using var doc = JsonDocument.Parse(stream);

        if (!doc.RootElement.TryGetProperty("sentences", out var sentences) || sentences.ValueKind != JsonValueKind.Array)
        {
            return new Dictionary<int, SentenceTiming>();
        }

        var timings = new Dictionary<int, AudioSentenceTiming>();
        foreach (var sentence in sentences.EnumerateArray())
        {
            if (!TryGetInt(sentence, "id", out var id))
            {
                continue;
            }

            if (!TryReadTiming(sentence, out var start, out var end))
            {
                continue;
            }

            timings[id] = new AudioSentenceTiming(start, end);
        }

        return timings;
    }

    private static bool TryReadTiming(JsonElement sentence, out double start, out double end)
    {
        start = double.NaN;
        end = double.NaN;

        if (sentence.TryGetProperty("timing", out var timingObj) && timingObj.ValueKind == JsonValueKind.Object)
        {
            if (TryGetDouble(timingObj, "startSec", out start) && TryGetDouble(timingObj, "endSec", out end))
            {
                return true;
            }

            if (TryGetDouble(timingObj, "start", out start) && TryGetDouble(timingObj, "end", out end))
            {
                return true;
            }
        }

        if (TryGetDouble(sentence, "startSec", out start) && TryGetDouble(sentence, "endSec", out end))
        {
            return true;
        }

        if (TryGetDouble(sentence, "start", out start) && TryGetDouble(sentence, "end", out end))
        {
            return true;
        }

        start = double.NaN;
        end = double.NaN;
        return false;
    }

    private static bool TryGetDouble(JsonElement element, string propertyName, out double value)
    {
        value = double.NaN;
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            return false;
        }

        if (prop.ValueKind == JsonValueKind.Number)
        {
            value = prop.GetDouble();
            return true;
        }

        if (prop.ValueKind == JsonValueKind.String && double.TryParse(prop.GetString(), out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            return false;
        }

        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out value))
        {
            return true;
        }

        if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out value))
        {
            return true;
        }

        return false;
    }

    private static void WriteVerificationCsv(string path, string chapterLabel, string variantLabel, AudioVerificationResult result)
    {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.WriteLine("chapter,variant,type,startSec,endSec,rawDb,treatedDb,deltaDb,sentenceIds");

        foreach (var mismatch in result.Mismatches)
        {
            var sentenceIds = mismatch.Sentences.Count > 0
                ? string.Join('|', mismatch.Sentences.Select(span => span.SentenceId))
                : string.Empty;

            writer.WriteLine(string.Join(
                ',',
                EscapeCsv(chapterLabel),
                EscapeCsv(variantLabel),
                mismatch.Type.ToString(),
                mismatch.StartSec.ToString("F6", CultureInfo.InvariantCulture),
                mismatch.EndSec.ToString("F6", CultureInfo.InvariantCulture),
                mismatch.RawDb.ToString("F2", CultureInfo.InvariantCulture),
                mismatch.TreatedDb.ToString("F2", CultureInfo.InvariantCulture),
                mismatch.DeltaDb.ToString("F2", CultureInfo.InvariantCulture),
                EscapeCsv(sentenceIds)));
        }
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        bool requiresQuotes = value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
        if (!requiresQuotes)
        {
            return value;
        }

        var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return "\"" + escaped + "\"";
    }

    private enum VerificationReportFormat
    {
        Json,
        Csv,
        Both
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
        var buffer = AudioProcessor.Decode(audioFile.FullName);
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
            Log.Debug("Failed to parse TextGrid for silences {Path}: {Message}", textGridFile.FullName, ex.Message);
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
        var markers = new[] { ".pause-adjusted", ".treated" };

        foreach (var marker in markers)
        {
            while (stem.EndsWith(marker, PathComparison) || stem.EndsWith(marker.TrimStart('.'), PathComparison))
            {
                var toTrim = stem.EndsWith(marker, PathComparison) ? marker.Length : marker.TrimStart('.').Length;
                if (stem.Length <= toTrim)
                {
                    break;
                }

                stem = stem[..^toTrim];
            }
        }

        return stem + source.Extension;
    }
}
