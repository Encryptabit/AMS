using System.CommandLine;
using Ams.Cli.Utilities;
using Ams.Core.Application.Commands;

namespace Ams.Cli.Commands;

public static class BuildIndexCommand
{
    public static Command Create(BuildBookIndexCommand buildBookIndex)
    {
        ArgumentNullException.ThrowIfNull(buildBookIndex);

        var buildIndexCommand = new Command("build-index", "Build book index from document files");

        var bookFileOption = new Option<FileInfo?>("--book", "Path to the book file (DOCX, TXT, MD, RTF)");
        bookFileOption.AddAlias("-b");

        var outputOption = new Option<FileInfo?>("--out", "Output book index JSON file");
        outputOption.AddAlias("-o");

        var forceCacheRefreshOption = new Option<bool>("--force-refresh", () => false,
            "Force cache refresh even if cached version exists");
        forceCacheRefreshOption.AddAlias("-f");

        var averageWpmOption =
            new Option<double>("--avg-wpm", () => 200.0, "Average words per minute for duration estimation");
        var noCacheOption = new Option<bool>("--no-cache", () => false,
            "Disable caching (don't read from or write to cache)");

        buildIndexCommand.AddOption(bookFileOption);
        buildIndexCommand.AddOption(outputOption);
        buildIndexCommand.AddOption(forceCacheRefreshOption);
        buildIndexCommand.AddOption(averageWpmOption);
        buildIndexCommand.AddOption(noCacheOption);

        buildIndexCommand.SetHandler(async context =>
        {
            try
            {
                var bookFile =
                    CommandInputResolver.ResolveBookSource(context.ParseResult.GetValueForOption(bookFileOption));
                var outputFile =
                    CommandInputResolver.ResolveBookIndex(context.ParseResult.GetValueForOption(outputOption),
                        mustExist: false);
                var forceRefresh = context.ParseResult.GetValueForOption(forceCacheRefreshOption);
                var averageWpm = context.ParseResult.GetValueForOption(averageWpmOption);
                var noCache = context.ParseResult.GetValueForOption(noCacheOption);

                var request = BuildBookIndexRequest.FromCliOptions(
                    bookFile,
                    outputFile,
                    forceRefresh,
                    noCache,
                    averageWpm);

                var result = await buildBookIndex.ExecuteAsync(request, context.GetCancellationToken())
                    .ConfigureAwait(false);

                Log.Debug(
                    "Book index completed with {CacheDisposition} (Rebuilt={WasRebuilt}, PhonemesBackfilled={PhonemesBackfilled})",
                    result.CacheDisposition,
                    result.WasRebuilt,
                    result.PhonemesBackfilled);

                Log.Debug(
                    "Book indexed: {SourceFile} (Words={WordCount:n0}, Sentences={SentenceCount:n0}, Paragraphs={ParagraphCount:n0}, Sections={SectionCount}, EstimatedDuration={EstimatedDuration})",
                    result.Index.SourceFile,
                    result.Index.Totals.Words,
                    result.Index.Totals.Sentences,
                    result.Index.Totals.Paragraphs,
                    result.Index.Sections.Length,
                    FormatDuration(result.Index.Totals.EstimatedDurationSec));

                Log.Debug("Book index saved to {OutputFile}", result.Artifacts[0].Path);
            }
            catch (BuildBookIndexCommandException ex)
            {
                var outputArtifact = ex.Artifacts.FirstOrDefault();
                Log.Error(
                    ex,
                    "build-index command failed (Module={ModuleId}, Stage={Stage}, Kind={Kind}, OutputExists={OutputExists}): {Message}",
                    ex.ModuleId.Value,
                    ex.Failure.Stage ?? "book_index",
                    ex.Failure.Kind,
                    outputArtifact?.Exists ?? false,
                    ex.Failure.Message);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "build-index command failed");
                Environment.Exit(1);
            }
        });

        return buildIndexCommand;
    }

    private static string FormatDuration(double totalSeconds)
    {
        var timeSpan = TimeSpan.FromSeconds(totalSeconds);

        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
        else if (timeSpan.TotalMinutes >= 1)
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        else
            return $"{timeSpan.Seconds}s";
    }
}
