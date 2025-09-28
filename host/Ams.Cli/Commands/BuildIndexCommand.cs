using System.CommandLine;
using System.Text.Json;
using Ams.Core;
using Ams.Core.Common;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class BuildIndexCommand
{
    public static Command Create()
    {
        var buildIndexCommand = new Command("build-index", "Build book index from document files");
        
        var bookFileOption = new Option<FileInfo?>("--book", "Path to the book file (DOCX, TXT, MD, RTF)");
        bookFileOption.AddAlias("-b");

        var outputOption = new Option<FileInfo?>("--out", "Output book index JSON file");
        outputOption.AddAlias("-o");
        
        var forceCacheRefreshOption = new Option<bool>("--force-refresh", () => false, "Force cache refresh even if cached version exists");
        forceCacheRefreshOption.AddAlias("-f");
        
        var averageWpmOption = new Option<double>("--avg-wpm", () => 200.0, "Average words per minute for duration estimation");
        var noCacheOption = new Option<bool>("--no-cache", () => false, "Disable caching (don't read from or write to cache)");
        
        buildIndexCommand.AddOption(bookFileOption);
        buildIndexCommand.AddOption(outputOption);
        buildIndexCommand.AddOption(forceCacheRefreshOption);
        buildIndexCommand.AddOption(averageWpmOption);
        buildIndexCommand.AddOption(noCacheOption);
        
        buildIndexCommand.SetHandler(async context =>
        {
            try
            {
                var bookFile = CommandInputResolver.ResolveBookSource(context.ParseResult.GetValueForOption(bookFileOption));
                var outputFile = CommandInputResolver.ResolveBookIndex(context.ParseResult.GetValueForOption(outputOption), mustExist: false);
                var forceRefresh = context.ParseResult.GetValueForOption(forceCacheRefreshOption);
                var averageWpm = context.ParseResult.GetValueForOption(averageWpmOption);
                var noCache = context.ParseResult.GetValueForOption(noCacheOption);
                
                await BuildBookIndexAsync(
                    bookFile,
                    outputFile,
                    forceRefresh,
                    new BookIndexOptions
                    {
                        AverageWpm = averageWpm
                    },
                    noCache);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "build-index command failed");
                Environment.Exit(1);
            }
        });
        
        return buildIndexCommand;
    }
    
    internal static async Task BuildBookIndexAsync(
        FileInfo bookFile,
        FileInfo outputFile,
        bool forceRefresh,
        BookIndexOptions options,
        bool noCache)
    {
        Log.Info("Building book index for {BookFile} -> {OutputFile}", bookFile.FullName, outputFile.FullName);

        if (!bookFile.Exists)
        {
            throw new FileNotFoundException($"Book file not found: {bookFile.FullName}");
        }

        var parser = new BookParser();
        var indexer = new BookIndexer();
        var cache = noCache ? null : new BookCache();

        if (!parser.CanParse(bookFile.FullName))
        {
            var supportedExts = string.Join(", ", parser.SupportedExtensions);
            throw new InvalidOperationException($"Unsupported file format. Supported formats: {supportedExts}");
        }

        BookIndex bookIndex;

        if (!forceRefresh && cache != null)
        {
            Log.Debug("Checking cache for {BookFile}", bookFile.FullName);
            var cachedIndex = await cache.GetAsync(bookFile.FullName);
            if (cachedIndex != null)
            {
                Log.Info("Cache hit for {BookFile}", bookFile.FullName);
                bookIndex = cachedIndex;
            }
            else
            {
                Log.Info("Cache miss for {BookFile}; rebuilding", bookFile.FullName);
                bookIndex = await ProcessBookFromScratch(parser, indexer, cache, bookFile.FullName, options);
            }
        }
        else
        {
            if (forceRefresh)
            {
                Log.Info("Force refresh requested for {BookFile}", bookFile.FullName);
            }
            else if (noCache)
            {
                Log.Info("Cache disabled for {BookFile}", bookFile.FullName);
            }

            bookIndex = await ProcessBookFromScratch(parser, indexer, cache, bookFile.FullName, options);
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(bookIndex, jsonOptions);
        await File.WriteAllTextAsync(outputFile.FullName, json);

        Log.Info(
            "Book indexed: {SourceFile} (Words={WordCount:n0}, Sentences={SentenceCount:n0}, Paragraphs={ParagraphCount:n0}, Sections={SectionCount}, EstimatedDuration={EstimatedDuration})",
            bookIndex.SourceFile,
            bookIndex.Totals.Words,
            bookIndex.Totals.Sentences,
            bookIndex.Totals.Paragraphs,
            bookIndex.Sections.Length,
            FormatDuration(bookIndex.Totals.EstimatedDurationSec));

        Log.Info("Book index saved to {OutputFile}", outputFile.FullName);
    }
    
    private static async Task<BookIndex> ProcessBookFromScratch(
        IBookParser parser,
        IBookIndexer indexer,
        IBookCache? cache,
        string bookFilePath,
        BookIndexOptions options)
    {
        Log.Info("Parsing book file {BookFile}", bookFilePath);
        var parseResult = await parser.ParseAsync(bookFilePath);
        Log.Debug("Parsed {CharacterCount:n0} characters from {BookFile}", parseResult.Text.Length, bookFilePath);
        
        Log.Info("Building index for {BookFile}", bookFilePath);
        var bookIndex = await indexer.CreateIndexAsync(parseResult, bookFilePath, options);
        Log.Debug("Index build complete for {BookFile}", bookFilePath);
        
        if (cache != null)
        {
            Log.Debug("Caching index for {BookFile}", bookFilePath);
            await cache.SetAsync(bookIndex);
            Log.Debug("Cache updated for {BookFile}", bookFilePath);
        }
        
        return bookIndex;
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
