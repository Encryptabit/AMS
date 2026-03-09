using System.CommandLine;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Cli.Repl;
using Ams.Cli.Utilities;
using Ams.Core.Audio.QualityControl;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Processors.Alignment.Anchors;
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
    private static readonly Regex FileNumberRegex = new(@"\d+", RegexOptions.Compiled);
    private static readonly Regex DecoratedChapterTitleRegex = new(
        @"^\s*chapter\b.+?[:\-–—]\s*.+\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly string[] VariantMarkers = [".pause-adjusted", ".treated", ".corrected", ".filtered"];

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
                var workspaceRoot = CommandInputResolver.ResolveDirectory(null);
                var workspaceChapters = TryDiscoverWorkspaceChapters(workspaceRoot);
                var bookIndex = TryLoadBookIndex();
                var rawLookup = BuildRawEquivalentLookup(workspaceChapters, bookIndex);

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
                        .Select(file => new { File = file, Key = GetSortKey(file) })
                        .OrderBy(entry => entry.Key.Category)
                        .ThenBy(entry => entry.Key.PrimaryNumber)
                        .ThenBy(entry => entry.Key.NameLower, StringComparer.Ordinal)
                        .ThenBy(entry => entry.File.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(entry => entry.File)
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
                    var chapters = workspaceChapters;

                    if (chapters.Count == 0)
                    {
                        Log.Error("No chapters found in workspace {Path}", workspaceRoot.FullName);
                        context.ExitCode = 1;
                        return;
                    }

                    var repl = ReplContext.Current;
                    var stem = repl?.ActiveChapterStem;

                    if (stem is null)
                    {
                        Log.Error("No chapter selected. Use 'use' to select a chapter or 'mode all' for batch analysis.");
                        context.ExitCode = 1;
                        return;
                    }

                    var chapter = chapters.FirstOrDefault(c =>
                        c.ChapterId.Equals(stem, StringComparison.OrdinalIgnoreCase));

                    if (chapter is null)
                    {
                        Log.Error("Chapter {ChapterId} not found in workspace", stem);
                        context.ExitCode = 1;
                        return;
                    }

                    var treated = chapter.AudioBuffers.FirstOrDefault(b => b.BufferId == "treated");
                    if (treated is null || !File.Exists(treated.Path))
                    {
                        Log.Error("Treated audio not found for {ChapterId}. Run the treatment pipeline first.", stem);
                        context.ExitCode = 1;
                        return;
                    }

                    files = [new FileInfo(treated.Path)];
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
                            file.FullName,
                            noiseDb,
                            minSilenceSec,
                            thresholds,
                            ResolveSectionTitle(file, bookIndex));

                        if (TryResolveRawDuration(file, rawLookup, result.DurationSec, out var rawDurationSec))
                        {
                            result = result with
                            {
                                RawDurationSec = rawDurationSec,
                                RuntimeDeltaSec = result.DurationSec - rawDurationSec
                            };
                        }

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
                RenderRuntimeSummary(results);

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
        table.AddColumn(new TableColumn("Delta") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Head") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Heading") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("DecGap") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Gap") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Tail") { Alignment = Justify.Right });
        table.AddColumn("Flags");

        foreach (var r in results)
        {
            var duration = FormatDuration(r.DurationSec);
            var delta = r.RuntimeDeltaSec.HasValue ? FormatSignedDuration(r.RuntimeDeltaSec.Value) : "-";
            var head = FormatSeconds(r.HeadSilenceSec);
            var title = r.TitleDurationSec.HasValue ? FormatSeconds(r.TitleDurationSec.Value) : "-";
            var decoratorGap = r.DecoratorGapSec.HasValue ? FormatSeconds(r.DecoratorGapSec.Value) : "-";
            var gap = r.TitleBodyGapSec.HasValue ? FormatSeconds(r.TitleBodyGapSec.Value) : "-";
            var tail = FormatSeconds(r.TailSilenceSec);

            var flagText = r.Flags.Count > 0
                ? $"[red]{Markup.Escape(string.Join(", ", r.Flags.Select(FormatFlagName)))}[/]"
                : "[green]OK[/]";

            table.AddRow(
                Markup.Escape(r.FileName),
                duration,
                delta,
                head,
                title,
                decoratorGap,
                gap,
                tail,
                flagText);
        }

        AnsiConsole.Write(table);
    }

    private static void RenderRuntimeSummary(List<ChapterQcResult> results)
    {
        var matched = results.Where(static r => r.RawDurationSec.HasValue).ToList();
        if (matched.Count == 0)
        {
            return;
        }

        var targetRuntimeSec = matched.Sum(static r => r.DurationSec);
        var rawRuntimeSec = matched.Sum(static r => r.RawDurationSec ?? 0.0);
        var deltaSec = targetRuntimeSec - rawRuntimeSec;
        var deltaColor = deltaSec switch
        {
            > 0.005 => "yellow",
            < -0.005 => "green",
            _ => "grey"
        };

        AnsiConsole.MarkupLine(
            "\n[bold]Runtime delta[/] (matched {0}/{1}): raw {2}, target {3}, [{4}]{5}[/]",
            matched.Count,
            results.Count,
            FormatDuration(rawRuntimeSec),
            FormatDuration(targetRuntimeSec),
            deltaColor,
            FormatSignedDuration(deltaSec));
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

    private static string FormatSignedDuration(double seconds)
    {
        var sign = seconds >= 0 ? "+" : "-";
        var absolute = Math.Abs(seconds);
        return sign + (absolute >= 60.0 ? FormatDuration(absolute) : FormatSeconds(absolute));
    }

    /// <summary>
    /// Extracts just the flag name (e.g. "HEAD_SILENCE_SHORT") from the full flag string.
    /// </summary>
    private static string FormatFlagName(string flag)
    {
        var parenIndex = flag.IndexOf('(');
        return parenIndex > 0 ? flag[..parenIndex].TrimEnd() : flag;
    }

    private static AudioFileSortKey GetSortKey(FileInfo file)
    {
        var stem = Path.GetFileNameWithoutExtension(file.Name);
        var match = FileNumberRegex.Match(stem);
        if (match.Success && int.TryParse(match.Value, out var primary))
        {
            return new AudioFileSortKey(0, primary, stem.ToLowerInvariant());
        }

        return new AudioFileSortKey(1, int.MaxValue, stem.ToLowerInvariant());
    }

    private static IReadOnlyList<ChapterDescriptor> TryDiscoverWorkspaceChapters(DirectoryInfo root)
    {
        try
        {
            return WorkspaceChapterDiscovery.Discover(root.FullName);
        }
        catch (Exception ex)
        {
            Log.Debug("Unable to discover workspace chapters from {Path}: {Message}", root.FullName, ex.Message);
            return [];
        }
    }

    private static BookIndex? TryLoadBookIndex()
    {
        try
        {
            var bookIndexFile = CommandInputResolver.ResolveBookIndex(null, mustExist: false);
            if (!bookIndexFile.Exists)
            {
                return null;
            }

            return JsonSerializer.Deserialize<BookIndex>(File.ReadAllText(bookIndexFile.FullName));
        }
        catch (Exception ex)
        {
            Log.Debug("Unable to load book-index.json for QC analysis: {Message}", ex.Message);
            return null;
        }
    }

    private static Dictionary<string, string> BuildRawEquivalentLookup(
        IReadOnlyList<ChapterDescriptor> chapters,
        BookIndex? bookIndex)
    {
        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var chapter in chapters)
        {
            var rawPath = chapter.AudioBuffers
                .FirstOrDefault(static buffer => string.Equals(buffer.BufferId, "raw", StringComparison.OrdinalIgnoreCase))
                ?.Path;
            if (string.IsNullOrWhiteSpace(rawPath) || !File.Exists(rawPath))
            {
                continue;
            }

            AddLookupKey(lookup, NormalizeChapterStem(chapter.ChapterId), rawPath);
            AddLookupKey(lookup, NormalizeChapterStem(Path.GetFileNameWithoutExtension(rawPath)), rawPath);

            var sectionTitle = ResolveSectionTitleFromChapter(chapter, bookIndex);
            if (!string.IsNullOrWhiteSpace(sectionTitle))
            {
                AddLookupKey(lookup, NormalizeChapterStem(sectionTitle), rawPath);
            }
        }

        return lookup;
    }

    private static void AddLookupKey(Dictionary<string, string> lookup, string key, string rawPath)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        lookup.TryAdd(key, rawPath);
    }

    private static string? ResolveSectionTitle(FileInfo file, BookIndex? bookIndex)
    {
        var stem = NormalizeChapterStem(Path.GetFileNameWithoutExtension(file.Name));
        if (bookIndex is not null)
        {
            var section = SectionLocator.ResolveSectionByTitle(bookIndex, stem);
            if (!string.IsNullOrWhiteSpace(section?.Title))
            {
                return section!.Title;
            }
        }

        return DecoratedChapterTitleRegex.IsMatch(stem) ? stem : null;
    }

    private static string? ResolveSectionTitleFromChapter(ChapterDescriptor chapter, BookIndex? bookIndex)
    {
        if (bookIndex is null)
        {
            return null;
        }

        var section = SectionLocator.ResolveSectionByTitle(bookIndex, chapter.ChapterId);
        return string.IsNullOrWhiteSpace(section?.Title) ? null : section!.Title;
    }

    private static bool TryResolveRawDuration(
        FileInfo analyzedFile,
        IReadOnlyDictionary<string, string> rawLookup,
        double analyzedDurationSec,
        out double rawDurationSec)
    {
        rawDurationSec = 0.0;
        var key = NormalizeChapterStem(Path.GetFileNameWithoutExtension(analyzedFile.Name));
        if (!rawLookup.TryGetValue(key, out var rawPath))
        {
            return false;
        }

        rawDurationSec = string.Equals(Path.GetFullPath(rawPath), analyzedFile.FullName, StringComparison.OrdinalIgnoreCase)
            ? analyzedDurationSec
            : AudioProcessor.Probe(rawPath).Duration.TotalSeconds;
        return true;
    }

    private static string NormalizeChapterStem(string stem)
    {
        var normalized = stem.Trim();
        bool removedMarker;
        do
        {
            removedMarker = false;
            foreach (var marker in VariantMarkers)
            {
                if (normalized.EndsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized[..^marker.Length].TrimEnd();
                    removedMarker = true;
                }
            }
        } while (removedMarker);

        return normalized;
    }

    private readonly record struct AudioFileSortKey(int Category, int PrimaryNumber, string NameLower);
}
