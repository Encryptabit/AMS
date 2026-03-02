using System.CommandLine;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Cli.Repl;
using Ams.Cli.Utilities;
using Ams.Core.Audio.QualityControl;
using Ams.Core.Common;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Workspace;
using Spectre.Console;

namespace Ams.Cli.Commands;

/// <summary>
/// CLI command for audiobook quality control analysis.
/// Workspace-aware by default (analyzes treated WAV files); use --dir to override.
/// </summary>
public static class QcCommand
{
    private static readonly string[] AudioExtensions = ["*.mp3", "*.wav", "*.flac", "*.m4a"];

    public static Command Create()
    {
        var qcCommand = new Command("qc", "Audiobook quality control tools");

        var analyzeCommand = CreateAnalyzeCommand();
        qcCommand.AddCommand(analyzeCommand);

        return qcCommand;
    }

    private static Command CreateAnalyzeCommand()
    {
        var cmd = new Command("analyze", "Analyze chapter audio files for structural QC (head/tail silence, title-body gap)");

        var dirOption = new Option<DirectoryInfo?>(
            "--dir",
            "Directory containing audio files to analyze (overrides workspace discovery)");
        dirOption.AddAlias("-d");

        var noiseOption = new Option<double>(
            "--noise",
            () => -40.0,
            "Silence detection noise floor in dB (default: -40, tuned for QC)");

        var minSilenceOption = new Option<double>(
            "--min-silence",
            () => 0.05,
            "Minimum silence duration in seconds for detection");

        var jsonOption = new Option<FileInfo?>(
            "--json",
            "Path to write JSON report");

        // Threshold overrides
        var minHeadOption = new Option<double>(
            "--min-head-silence",
            () => 0.5,
            "Minimum acceptable head silence in seconds");
        var maxHeadOption = new Option<double>(
            "--max-head-silence",
            () => 1.0,
            "Maximum acceptable head silence in seconds");
        var minTailOption = new Option<double>(
            "--min-tail-silence",
            () => 2.0,
            "Minimum acceptable tail silence in seconds");
        var maxTailOption = new Option<double>(
            "--max-tail-silence",
            () => 5.0,
            "Maximum acceptable tail silence in seconds");
        var minGapOption = new Option<double>(
            "--min-title-gap",
            () => 1.0,
            "Minimum acceptable title-body gap in seconds");
        var maxGapOption = new Option<double>(
            "--max-title-gap",
            () => 2.5,
            "Maximum acceptable title-body gap in seconds");

        cmd.AddOption(dirOption);
        cmd.AddOption(noiseOption);
        cmd.AddOption(minSilenceOption);
        cmd.AddOption(jsonOption);
        cmd.AddOption(minHeadOption);
        cmd.AddOption(maxHeadOption);
        cmd.AddOption(minTailOption);
        cmd.AddOption(maxTailOption);
        cmd.AddOption(minGapOption);
        cmd.AddOption(maxGapOption);

        cmd.SetHandler(async context =>
        {
            var ct = context.GetCancellationToken();

            try
            {
                var dir = context.ParseResult.GetValueForOption(dirOption);
                var noiseDb = context.ParseResult.GetValueForOption(noiseOption);
                var minSilenceSec = context.ParseResult.GetValueForOption(minSilenceOption);
                var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

                var thresholds = new QcThresholds
                {
                    MinHeadSilence = context.ParseResult.GetValueForOption(minHeadOption),
                    MaxHeadSilence = context.ParseResult.GetValueForOption(maxHeadOption),
                    MinTailSilence = context.ParseResult.GetValueForOption(minTailOption),
                    MaxTailSilence = context.ParseResult.GetValueForOption(maxTailOption),
                    MinTitleBodyGap = context.ParseResult.GetValueForOption(minGapOption),
                    MaxTitleBodyGap = context.ParseResult.GetValueForOption(maxGapOption)
                };

                List<FileInfo> files;

                if (dir is not null)
                {
                    // --dir override: scan directory for audio files
                    if (!dir.Exists)
                    {
                        Log.Error("Directory not found: {Path}", dir.FullName);
                        context.ExitCode = 1;
                        return;
                    }

                    files = AudioExtensions
                        .SelectMany(ext => dir.GetFiles(ext))
                        .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (files.Count == 0)
                    {
                        Log.Error("No audio files found in {Path} (supported: {Extensions})",
                            dir.FullName, string.Join(", ", AudioExtensions));
                        context.ExitCode = 1;
                        return;
                    }

                    Log.Info("Found {Count} audio files in {Path}", files.Count, dir.FullName);
                }
                else
                {
                    // Workspace-aware mode: discover treated WAV files
                    var root = CommandInputResolver.ResolveDirectory(null);
                    var chapters = WorkspaceChapterDiscovery.Discover(root.FullName);

                    if (chapters.Count == 0)
                    {
                        Log.Error("No chapters found in workspace {Path}", root.FullName);
                        context.ExitCode = 1;
                        return;
                    }

                    var repl = ReplContext.Current;
                    IEnumerable<ChapterDescriptor> targetChapters;

                    if (repl is null || repl.RunAllChapters)
                    {
                        // Standalone CLI or REPL in "mode all"
                        targetChapters = chapters;
                    }
                    else if (repl.ActiveChapterStem is { } stem)
                    {
                        // Single chapter selected
                        targetChapters = chapters.Where(c =>
                            c.ChapterId.Equals(stem, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        Log.Error("No chapter selected. Use 'use' to select a chapter or 'mode all' for batch analysis.");
                        context.ExitCode = 1;
                        return;
                    }

                    files = [];
                    foreach (var chapter in targetChapters)
                    {
                        var treated = chapter.AudioBuffers.FirstOrDefault(b => b.BufferId == "treated");
                        if (treated is null)
                            continue;

                        if (File.Exists(treated.Path))
                        {
                            files.Add(new FileInfo(treated.Path));
                        }
                        else
                        {
                            Log.Debug("Skipping {ChapterId}: treated audio not found at {Path}",
                                chapter.ChapterId, treated.Path);
                        }
                    }

                    if (files.Count == 0)
                    {
                        Log.Error("No treated audio files found in workspace {Path}. Run the treatment pipeline first.",
                            root.FullName);
                        context.ExitCode = 1;
                        return;
                    }

                    Log.Info("Found {Count} treated audio files in workspace", files.Count);
                }

                // Analyze each file
                var results = new List<ChapterQcResult>();
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    Log.Info("Analyzing {Index}/{Total}: {FileName}...",
                        i + 1, files.Count, file.Name);

                    try
                    {
                        var result = AudioQcAnalyzer.AnalyzeFile(
                            file.FullName, noiseDb, minSilenceSec, thresholds);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to analyze {FileName}", file.Name);
                        results.Add(new ChapterQcResult
                        {
                            FileName = file.Name,
                            DurationSec = 0,
                            Flags = ["ANALYSIS_FAILED"]
                        });
                    }
                }

                // Render console table
                RenderTable(results);

                // Summary line
                var flaggedCount = results.Count(r => r.Flags.Count > 0);
                AnsiConsole.MarkupLine($"\n[bold]{results.Count}[/] files analyzed, [bold]{flaggedCount}[/] flagged");

                // JSON export
                if (jsonOutput is not null)
                {
                    var jsonOptions = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var json = JsonSerializer.Serialize(results, jsonOptions);
                    var outputDir = jsonOutput.Directory;
                    if (outputDir is not null && !outputDir.Exists)
                    {
                        outputDir.Create();
                    }
                    await File.WriteAllTextAsync(jsonOutput.FullName, json, ct);
                    Log.Info("JSON report written to {Path}", jsonOutput.FullName);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Debug("QC analysis cancelled");
                context.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "QC analysis failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static void RenderTable(List<ChapterQcResult> results)
    {
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.Title = new TableTitle("[bold]Audiobook QC Analysis[/]");

        table.AddColumn(new TableColumn("File") { NoWrap = true });
        table.AddColumn(new TableColumn("Duration") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Head") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Title") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Gap") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Tail") { Alignment = Justify.Right });
        table.AddColumn("Flags");

        foreach (var r in results)
        {
            var duration = FormatDuration(r.DurationSec);
            var head = FormatSeconds(r.HeadSilenceSec);
            var title = r.TitleDurationSec.HasValue ? FormatSeconds(r.TitleDurationSec.Value) : "-";
            var gap = r.TitleBodyGapSec.HasValue ? FormatSeconds(r.TitleBodyGapSec.Value) : "-";
            var tail = FormatSeconds(r.TailSilenceSec);

            var flagText = r.Flags.Count > 0
                ? $"[red]{Markup.Escape(string.Join(", ", r.Flags.Select(FormatFlagName)))}[/]"
                : "[green]OK[/]";

            table.AddRow(
                Markup.Escape(r.FileName),
                duration,
                head,
                title,
                gap,
                tail,
                flagText);
        }

        AnsiConsole.Write(table);
    }

    private static string FormatDuration(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss\.f", CultureInfo.InvariantCulture)
            : ts.ToString(@"mm\:ss\.f", CultureInfo.InvariantCulture);
    }

    private static string FormatSeconds(double seconds)
    {
        return seconds.ToString("F2", CultureInfo.InvariantCulture) + "s";
    }

    /// <summary>
    /// Extracts just the flag name (e.g. "HEAD_SILENCE_SHORT") from the full flag string.
    /// </summary>
    private static string FormatFlagName(string flag)
    {
        var parenIndex = flag.IndexOf('(');
        return parenIndex > 0 ? flag[..parenIndex].TrimEnd() : flag;
    }
}
