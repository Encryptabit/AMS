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
    private static readonly Regex DecoratedChapterTitleRegex = new(
        @"^\s*chapter\b.+?[:\-–—]\s*.+\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly string[] VariantMarkers =
    [
        ".dsp.filtered",
        ".pause-adjusted",
        ".corrected",
        ".treated",
        ".filtered",
        ".dsp"
    ];

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

        var tierOption = new Option<string?>(
            "--tier",
            "Audio tier to analyze: source, treated, filtered, adjusted. Defaults to treated in workspace mode; with --dir, omit to analyze all files.");

        var noiseOption = new Option<double>(
            "--noise",
            () => -55.0,
            "Silence detection noise floor in dB (default: -55, matches treat's silence threshold)");

        var minSilenceOption = new Option<double>(
            "--min-silence",
            () => 0.25,
            "Minimum silence duration in seconds for detection (default: 0.25)");

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
            () => 4.0,
            "Maximum acceptable tail silence in seconds");
        var minGapOption = new Option<double>(
            "--min-title-gap",
            () => 1.0,
            "Minimum acceptable title-body gap in seconds");
        var maxGapOption = new Option<double>(
            "--max-title-gap",
            () => 2.0,
            "Maximum acceptable title-body gap in seconds");

        cmd.AddOption(dirOption);
        cmd.AddOption(tierOption);
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
                var tierValue = context.ParseResult.GetValueForOption(tierOption);
                var tierSpecified = context.ParseResult.FindResultFor(tierOption) is not null
                                    && !string.IsNullOrWhiteSpace(tierValue);
                var noiseDb = context.ParseResult.GetValueForOption(noiseOption);
                var minSilenceSec = context.ParseResult.GetValueForOption(minSilenceOption);
                var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);
                var workspaceRoot = ResolveQcWorkspaceRoot(dir);
                var workspaceChapters = TryDiscoverWorkspaceChapters(workspaceRoot);
                var bookIndex = TryLoadBookIndex(workspaceRoot);
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
                    // --dir override: by default preserve legacy flat-directory behavior.
                    // If --tier is supplied, make the directory scan tier-aware and recursive.
                    var directoryTier = tierSpecified
                        ? AudioTierResolver.Parse(tierValue, AudioTier.Treated, allowAdjusted: true)
                        : (AudioTier?)null;

                    if (!dir.Exists)
                    {
                        Log.Error("Directory not found: {Path}", dir.FullName);
                        context.ExitCode = 1;
                        return;
                    }

                    files = tierSpecified
                        ? DiscoverDirectoryTierFiles(dir, directoryTier!.Value)
                        : DiscoverDirectoryAudioFiles(dir);

                    if (files.Count == 0)
                    {
                        if (directoryTier.HasValue)
                        {
                            Log.Error("No {Tier} audio files found in {Path}",
                                AudioTierResolver.Describe(directoryTier.Value),
                                dir.FullName);
                        }
                        else
                        {
                            Log.Error("No audio files found in {Path} (supported: {Extensions})",
                                dir.FullName,
                                string.Join(", ", AudioExtensions));
                        }

                        context.ExitCode = 1;
                        return;
                    }

                    if (directoryTier.HasValue)
                    {
                        Log.Info("Found {Count} {Tier} audio files in {Path}",
                            files.Count,
                            AudioTierResolver.Describe(directoryTier.Value),
                            dir.FullName);
                    }
                    else
                    {
                        Log.Info("Found {Count} audio files in {Path}", files.Count, dir.FullName);
                    }
                }
                else
                {
                    // Workspace-aware mode: discover the requested tier for the selected chapter,
                    // or all chapters when the REPL is in mode all.
                    var tier = AudioTierResolver.Parse(tierValue, AudioTier.Treated, allowAdjusted: true);
                    var chapters = workspaceChapters;

                    if (chapters.Count == 0)
                    {
                        Log.Error("No chapters found in workspace {Path}", workspaceRoot.FullName);
                        context.ExitCode = 1;
                        return;
                    }

                    var repl = ReplContext.Current;
                    var stem = repl?.ActiveChapterStem;

                    if (stem is null && repl?.RunAllChapters != true)
                    {
                        Log.Error("No chapter selected. Use 'use' to select a chapter or 'mode all' for batch analysis.");
                        context.ExitCode = 1;
                        return;
                    }

                    files = ResolveWorkspaceTierFiles(
                        chapters,
                        tier,
                        repl?.RunAllChapters == true ? null : stem);
                    if (files.Count == 0)
                    {
                        Log.Error("{Tier} audio not found for {Scope}",
                            AudioTierResolver.Describe(tier),
                            repl?.RunAllChapters == true ? "any chapter" : stem);
                        context.ExitCode = 1;
                        return;
                    }

                    Log.Info("Found {Count} {Tier} file(s) in workspace {Path}",
                        files.Count,
                        AudioTierResolver.Describe(tier),
                        workspaceRoot.FullName);
                }

                // Analyze files in parallel; results assigned by index to preserve sort order.
                var resultsByIndex = new ChapterQcResult[files.Count];
                var completed = 0;
                Parallel.For(0, files.Count,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount,
                        CancellationToken = ct
                    },
                    i =>
                    {
                        var file = files[i];
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

                            resultsByIndex[i] = result;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to analyze {FileName}", file.Name);
                            resultsByIndex[i] = new ChapterQcResult
                            {
                                FileName = file.Name,
                                DurationSec = 0,
                                Flags = ["ANALYSIS_FAILED"]
                            };
                        }

                        var done = Interlocked.Increment(ref completed);
                        Log.Info("Analyzed {Index}/{Total}: {FileName}", done, files.Count, file.Name);
                    });

                var results = resultsByIndex.ToList();

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

    internal static List<FileInfo> DiscoverDirectoryAudioFiles(DirectoryInfo directory)
    {
        ArgumentNullException.ThrowIfNull(directory);

        return AudioExtensions
            .SelectMany(ext => directory.GetFiles(ext))
            .OrderBy(file => file, NaturalStringComparer.FileNameWithoutExtensionIgnoreCase)
            .ToList();
    }

    internal static List<FileInfo> DiscoverDirectoryTierFiles(DirectoryInfo directory, AudioTier tier)
    {
        ArgumentNullException.ThrowIfNull(directory);

        var files = tier == AudioTier.Source
            ? AudioExtensions.SelectMany(ext => directory.EnumerateFiles(ext, SearchOption.AllDirectories))
                .Where(file => !IsInSkippedDiscoveryDirectory(directory, file.FullName))
                .Where(IsSourceTierFile)
            : directory.EnumerateFiles(TierSearchPattern(tier), SearchOption.AllDirectories)
                .Where(file => !IsInSkippedDiscoveryDirectory(directory, file.FullName));

        return files
            .OrderBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<FileInfo> ResolveWorkspaceTierFiles(
        IReadOnlyList<ChapterDescriptor> chapters,
        AudioTier tier,
        string? chapterStem)
    {
        var selected = chapters.Where(chapter => !IsSkippedWorkspaceChapter(chapter));
        if (!string.IsNullOrWhiteSpace(chapterStem))
        {
            selected = selected.Where(chapter =>
                chapter.ChapterId.Equals(chapterStem, StringComparison.OrdinalIgnoreCase));
        }

        var files = new List<FileInfo>();
        foreach (var chapter in selected)
        {
            var file = ResolveWorkspaceTierFile(chapter, tier);
            if (file?.Exists == true)
            {
                files.Add(file);
            }
            else
            {
                Log.Debug("{Tier} audio missing for {ChapterId}",
                    AudioTierResolver.Describe(tier),
                    chapter.ChapterId);
            }
        }

        return files
            .OrderBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static FileInfo? ResolveWorkspaceTierFile(ChapterDescriptor chapter, AudioTier tier)
    {
        var bufferId = tier switch
        {
            AudioTier.Source => "raw",
            AudioTier.Treated => "treated",
            AudioTier.Filtered => "filtered",
            AudioTier.Adjusted => null,
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
        };

        if (bufferId is not null)
        {
            var path = chapter.AudioBuffers
                .FirstOrDefault(buffer => string.Equals(buffer.BufferId, bufferId, StringComparison.OrdinalIgnoreCase))
                ?.Path;
            return string.IsNullOrWhiteSpace(path) ? null : new FileInfo(path);
        }

        var suffix = AudioTierResolver.ArtifactSuffix(tier);
        return string.IsNullOrWhiteSpace(suffix)
            ? null
            : new FileInfo(Path.Combine(chapter.RootPath, $"{chapter.ChapterId}.{suffix}"));
    }

    private static bool IsSourceTierFile(FileInfo file)
        => !AudioTierResolver.IsVariantFileName(file.Name)
           && !file.Name.Equals("roomtone.wav", StringComparison.OrdinalIgnoreCase);

    private static string TierSearchPattern(AudioTier tier) => tier switch
    {
        AudioTier.Treated => "*.treated.wav",
        AudioTier.Filtered => "*.filtered.wav",
        AudioTier.Adjusted => "*.pause-adjusted.wav",
        AudioTier.Source => "*.wav",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
    };

    private static bool IsInSkippedDiscoveryDirectory(DirectoryInfo root, string candidatePath)
    {
        var relative = Path.GetRelativePath(root.FullName, candidatePath);
        if (relative.StartsWith("..", StringComparison.Ordinal))
        {
            return false;
        }

        var parts = relative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        return parts.Take(Math.Max(0, parts.Length - 1)).Any(IsSkippedDiscoveryDirectoryName);
    }

    private static bool IsSkippedWorkspaceChapter(ChapterDescriptor chapter)
    {
        if (IsSkippedDiscoveryDirectoryName(chapter.ChapterId))
        {
            return true;
        }

        var directoryName = Path.GetFileName(
            chapter.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return IsSkippedDiscoveryDirectoryName(directoryName);
    }

    private static bool IsSkippedDiscoveryDirectoryName(string? name)
        => string.Equals(name, "safe", StringComparison.OrdinalIgnoreCase)
           || string.Equals(name, "Batch 2", StringComparison.OrdinalIgnoreCase)
           || string.Equals(name, "CRX", StringComparison.OrdinalIgnoreCase)
           || (!string.IsNullOrWhiteSpace(name) && name.StartsWith(".", StringComparison.Ordinal));

    private static void RenderTable(List<ChapterQcResult> results)
    {
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.Title = new TableTitle("[bold]Audiobook QC Analysis[/]");

        table.AddColumn(new TableColumn("File") { NoWrap = true });
        table.AddColumn(new TableColumn("Duration") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Delta") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("RMS") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("Peak") { Alignment = Justify.Right });
        table.AddColumn(new TableColumn("LUFS") { Alignment = Justify.Right });
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
            var rms = FormatDbFs(r.OverallRmsDbFs);
            var peak = FormatDbFs(r.TruePeakDbFs ?? r.SamplePeakDbFs);
            var lufs = FormatLufs(r.IntegratedLufs);
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
                rms,
                peak,
                lufs,
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

    private static string FormatDbFs(double? dbFs)
    {
        return dbFs.HasValue
            ? dbFs.Value.ToString("F1", CultureInfo.InvariantCulture)
            : "-";
    }

    private static string FormatLufs(double? lufs)
    {
        return lufs.HasValue
            ? lufs.Value.ToString("F1", CultureInfo.InvariantCulture)
            : "-";
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

    private static IReadOnlyList<ChapterDescriptor> TryDiscoverWorkspaceChapters(DirectoryInfo root)
    {
        try
        {
            return WorkspaceChapterDiscovery.Discover(root.FullName)
                .Where(chapter => !IsSkippedWorkspaceChapter(chapter))
                .ToList();
        }
        catch (Exception ex)
        {
            Log.Debug("Unable to discover workspace chapters from {Path}: {Message}", root.FullName, ex.Message);
            return [];
        }
    }

    private static DirectoryInfo ResolveQcWorkspaceRoot(DirectoryInfo? directoryOverride)
    {
        if (directoryOverride is null)
        {
            return CommandInputResolver.ResolveDirectory(null);
        }

        var directory = new DirectoryInfo(Path.GetFullPath(directoryOverride.FullName));
        if (File.Exists(Path.Combine(directory.FullName, "book-index.json")))
        {
            return directory;
        }

        if (directory.Parent is { } parent &&
            File.Exists(Path.Combine(parent.FullName, "book-index.json")))
        {
            return parent;
        }

        return CommandInputResolver.ResolveDirectory(null);
    }

    private static BookIndex? TryLoadBookIndex(DirectoryInfo root)
    {
        try
        {
            var bookIndexFile = new FileInfo(Path.Combine(root.FullName, "book-index.json"));
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
}
