using System.CommandLine;
using System.Text.Json;
using Ams.Core;

namespace Ams.Cli.Commands;

public static class BuildIndexCommand
{
    public static Command Create()
    {
        var buildIndexCommand = new Command("build-index", "Build book index from document files");
        
        var bookFileOption = new Option<FileInfo>("--book", "Path to the book file (DOCX, TXT, MD, RTF)")
        {
            IsRequired = true
        };
        bookFileOption.AddAlias("-b");
        
        var outputOption = new Option<FileInfo>("--out", "Output book index JSON file")
        {
            IsRequired = true
        };
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
        
        buildIndexCommand.SetHandler(async (context) =>
        {
            try
            {
                var bookFile = context.ParseResult.GetValueForOption(bookFileOption)!;
                var outputFile = context.ParseResult.GetValueForOption(outputOption)!;
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
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });
        
        return buildIndexCommand;
    }
    
    private static async Task BuildBookIndexAsync(
        FileInfo bookFile, 
        FileInfo outputFile,
        bool forceRefresh,
        BookIndexOptions options,
        bool noCache)
    {
        Console.WriteLine($"Building book index...");
        Console.WriteLine($"Book file: {bookFile.FullName}");
        Console.WriteLine($"Output file: {outputFile.FullName}");
        
        // Validate input files
        if (!bookFile.Exists)
            throw new FileNotFoundException($"Book file not found: {bookFile.FullName}");
        
        
        
        // Initialize services
        var parser = new BookParser();
        var indexer = new BookIndexer();
        var cache = noCache ? null : new BookCache();
        
        if (!parser.CanParse(bookFile.FullName))
        {
            var supportedExts = string.Join(", ", parser.SupportedExtensions);
            throw new InvalidOperationException($"Unsupported file format. Supported formats: {supportedExts}");
        }
        
        BookIndex bookIndex;
        
        // Try to load from cache if not forcing refresh and cache is enabled
        if (!forceRefresh && cache != null)
        {
            Console.Write("Checking cache... ");
            var cachedIndex = await cache.GetAsync(bookFile.FullName);
            if (cachedIndex != null)
            {
                Console.WriteLine("Found valid cache");
                bookIndex = cachedIndex;
            }
            else
            {
                Console.WriteLine("No valid cache found");
                bookIndex = await ProcessBookFromScratch(parser, indexer, cache, bookFile.FullName, options);
            }
        }
        else
        {
            if (forceRefresh)
                Console.WriteLine("Force refresh enabled, ignoring cache");
            else if (noCache)
                Console.WriteLine("Cache disabled");
            
            bookIndex = await ProcessBookFromScratch(parser, indexer, cache, bookFile.FullName, options);
        }
        
        // Save the final index
        Console.Write("Saving book index... ");
        var jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(bookIndex, jsonOptions);
        await File.WriteAllTextAsync(outputFile.FullName, json);
        Console.WriteLine("Done");
        
        // Print summary
        Console.WriteLine("\n=== Book Index Summary ===");
        Console.WriteLine($"Title: {bookIndex.Title ?? "(not available)"}");
        Console.WriteLine($"Author: {bookIndex.Author ?? "(not available)"}");
        Console.WriteLine($"Source file: {bookIndex.SourceFile}");
        Console.WriteLine($"Source file hash: {bookIndex.SourceFileHash[..16]}...");
        Console.WriteLine($"Indexed at: {bookIndex.IndexedAt:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine($"Total words: {bookIndex.Totals.Words:n0}");
        Console.WriteLine($"Total sentences: {bookIndex.Totals.Sentences:n0}");
        Console.WriteLine($"Total paragraphs: {bookIndex.Totals.Paragraphs:n0}");
        Console.WriteLine($"Estimated duration: {FormatDuration(bookIndex.Totals.EstimatedDurationSec)}");
        Console.WriteLine($"Sections (Heading 1): {bookIndex.Sections.Length}");
        
        Console.WriteLine($"\nBook index saved to: {outputFile.FullName}");
    }
    
    private static async Task<BookIndex> ProcessBookFromScratch(
        IBookParser parser,
        IBookIndexer indexer,
        IBookCache? cache,
        string bookFilePath,
        BookIndexOptions options)
    {
        Console.Write("Parsing book file... ");
        var parseResult = await parser.ParseAsync(bookFilePath);
        Console.WriteLine($"Done ({parseResult.Text.Length:n0} characters)");
        
        Console.Write("Building index... ");
        var bookIndex = await indexer.CreateIndexAsync(parseResult, bookFilePath, options);
        Console.WriteLine("Done");
        
        // Cache the result if cache is enabled
        if (cache != null)
        {
            Console.Write("Caching result... ");
            await cache.SetAsync(bookIndex);
            Console.WriteLine("Done");
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
