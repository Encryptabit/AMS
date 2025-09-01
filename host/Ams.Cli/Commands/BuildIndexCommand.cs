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
        
        var asrJsonOption = new Option<FileInfo?>("--asr-json", "Optional ASR JSON file for timing alignment")
        {
            IsRequired = false
        };
        asrJsonOption.AddAlias("-a");
        
        var forceCacheRefreshOption = new Option<bool>("--force-refresh", () => false, "Force cache refresh even if cached version exists");
        forceCacheRefreshOption.AddAlias("-f");
        
        var averageWpmOption = new Option<double>("--avg-wpm", () => 200.0, "Average words per minute for duration estimation");
        var extractMetadataOption = new Option<bool>("--extract-metadata", () => true, "Extract document metadata (title, author)");
        var normalizeTextOption = new Option<bool>("--normalize-text", () => false, "Normalize text (whitespace, punctuation)");
        var includeParagraphsOption = new Option<bool>("--include-paragraphs", () => true, "Include paragraph segments");
        var minSentenceLengthOption = new Option<int>("--min-sentence-length", () => 5, "Minimum characters for valid sentences");
        var minParagraphWordsOption = new Option<int>("--min-paragraph-words", () => 2, "Minimum words for valid paragraphs");
        var noCacheOption = new Option<bool>("--no-cache", () => false, "Disable caching (don't read from or write to cache)");
        
        buildIndexCommand.AddOption(bookFileOption);
        buildIndexCommand.AddOption(outputOption);
        buildIndexCommand.AddOption(asrJsonOption);
        buildIndexCommand.AddOption(forceCacheRefreshOption);
        buildIndexCommand.AddOption(averageWpmOption);
        buildIndexCommand.AddOption(extractMetadataOption);
        buildIndexCommand.AddOption(normalizeTextOption);
        buildIndexCommand.AddOption(includeParagraphsOption);
        buildIndexCommand.AddOption(minSentenceLengthOption);
        buildIndexCommand.AddOption(minParagraphWordsOption);
        buildIndexCommand.AddOption(noCacheOption);
        
        buildIndexCommand.SetHandler(async (context) =>
        {
            try
            {
                var bookFile = context.ParseResult.GetValueForOption(bookFileOption)!;
                var outputFile = context.ParseResult.GetValueForOption(outputOption)!;
                var asrJsonFile = context.ParseResult.GetValueForOption(asrJsonOption);
                var forceRefresh = context.ParseResult.GetValueForOption(forceCacheRefreshOption);
                var averageWpm = context.ParseResult.GetValueForOption(averageWpmOption);
                var extractMetadata = context.ParseResult.GetValueForOption(extractMetadataOption);
                var normalizeText = context.ParseResult.GetValueForOption(normalizeTextOption);
                var includeParagraphs = context.ParseResult.GetValueForOption(includeParagraphsOption);
                var minSentenceLength = context.ParseResult.GetValueForOption(minSentenceLengthOption);
                var minParagraphWords = context.ParseResult.GetValueForOption(minParagraphWordsOption);
                var noCache = context.ParseResult.GetValueForOption(noCacheOption);
                
                await BuildBookIndexAsync(
                    bookFile, 
                    outputFile, 
                    asrJsonFile, 
                    forceRefresh,
                    new BookIndexOptions
                    {
                        AverageWpm = averageWpm,
                        ExtractMetadata = extractMetadata,
                        NormalizeText = normalizeText,
                        IncludeParagraphSegments = includeParagraphs,
                        MinimumSentenceLength = minSentenceLength,
                        MinimumParagraphWords = minParagraphWords
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
        FileInfo? asrJsonFile,
        bool forceRefresh,
        BookIndexOptions options,
        bool noCache)
    {
        Console.WriteLine($"Building book index...");
        Console.WriteLine($"Book file: {bookFile.FullName}");
        Console.WriteLine($"Output file: {outputFile.FullName}");
        if (asrJsonFile != null)
            Console.WriteLine($"ASR file: {asrJsonFile.FullName}");
        
        // Validate input files
        if (!bookFile.Exists)
            throw new FileNotFoundException($"Book file not found: {bookFile.FullName}");
        
        if (asrJsonFile != null && !asrJsonFile.Exists)
            throw new FileNotFoundException($"ASR JSON file not found: {asrJsonFile.FullName}");
        
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
        
        // Apply ASR timing if provided
        if (asrJsonFile != null)
        {
            Console.Write("Loading ASR data... ");
            var asrJson = await File.ReadAllTextAsync(asrJsonFile.FullName);
            var asrResponse = JsonSerializer.Deserialize<AsrResponse>(asrJson);
            
            if (asrResponse?.Tokens == null || asrResponse.Tokens.Length == 0)
                throw new InvalidOperationException("ASR JSON file contains no valid tokens");
            
            Console.WriteLine($"Loaded {asrResponse.Tokens.Length} ASR tokens");
            
            Console.Write("Aligning timing data... ");
            bookIndex = await indexer.UpdateTimingAsync(bookIndex, asrResponse.Tokens);
            Console.WriteLine("Done");
            
            // Update cache with timing-aligned version if cache is enabled
            if (cache != null)
            {
                Console.Write("Updating cache with timing data... ");
                await cache.SetAsync(bookIndex);
                Console.WriteLine("Done");
            }
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
        Console.WriteLine($"Total words: {bookIndex.TotalWords:n0}");
        Console.WriteLine($"Total sentences: {bookIndex.TotalSentences:n0}");
        Console.WriteLine($"Total paragraphs: {bookIndex.TotalParagraphs:n0}");
        Console.WriteLine($"Estimated duration: {FormatDuration(bookIndex.EstimatedDuration)}");
        
        var wordsWithTiming = bookIndex.Words.Count(w => w.StartTime.HasValue);
        var segmentsWithTiming = bookIndex.Segments.Count(s => s.StartTime.HasValue);
        
        if (wordsWithTiming > 0 || segmentsWithTiming > 0)
        {
            Console.WriteLine("\n=== Timing Information ===");
            Console.WriteLine($"Words with timing: {wordsWithTiming:n0} ({wordsWithTiming / (double)bookIndex.TotalWords:P1})");
            Console.WriteLine($"Segments with timing: {segmentsWithTiming:n0} ({segmentsWithTiming / (double)bookIndex.Segments.Length:P1})");
            
            if (wordsWithTiming > 0)
            {
                var avgConfidence = bookIndex.Words
                    .Where(w => w.Confidence.HasValue)
                    .DefaultIfEmpty()
                    .Average(w => w.Confidence ?? 0.0);
                
                if (avgConfidence > 0)
                    Console.WriteLine($"Average timing confidence: {avgConfidence:F3}");
            }
        }
        
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