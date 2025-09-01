using System.Text;
using Ams.Core;

namespace Ams.Tests;

public class BookParsingTests
{
    [Fact]
    public async Task BookParserXceed_CanParse_ReturnsCorrectResult()
    {
        // Arrange
        var parser = new BookParser();
        
        // Act & Assert
        Assert.True(parser.CanParse("test.docx"));
        Assert.True(parser.CanParse("test.txt"));
        Assert.True(parser.CanParse("test.md"));
        Assert.True(parser.CanParse("test.rtf"));
        Assert.False(parser.CanParse("test.pdf"));
        Assert.False(parser.CanParse(""));
        Assert.False(parser.CanParse(null!));
    }
    
    [Fact]
    public async Task BookParserXceed_ParseTextFile_ReturnsValidResult()
    {
        // Arrange
        var parser = new BookParser();
        var tempFile = Path.GetTempFileName() + ".txt";
        var testContent = "Chapter 1\n\nThis is the first paragraph of the book.\n\nThis is the second paragraph with more content.";
        
        try
        {
            await File.WriteAllTextAsync(tempFile, testContent);
            
            // Act
            var result = await parser.ParseAsync(tempFile);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Text);
            Assert.Equal("Chapter 1", result.Title); // Should extract first line as title
            Assert.Null(result.Author);
            Assert.NotNull(result.Metadata);
            Assert.True(result.Metadata.ContainsKey("fileSize"));
            Assert.True(result.Metadata.ContainsKey("encoding"));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public async Task BookParserXceed_ParseMarkdownFile_RemovesFormatting()
    {
        // Arrange
        var parser = new BookParser();
        var tempFile = Path.GetTempFileName() + ".md";
        var testContent = "# My Book Title\n\n## Chapter 1\n\nThis is **bold** and *italic* text.\n\n```code block```\n\n[Link](http://example.com) and `inline code`.";
        
        try
        {
            await File.WriteAllTextAsync(tempFile, testContent);
            
            // Act
            var result = await parser.ParseAsync(tempFile);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("My Book Title", result.Title);
            Assert.NotEmpty(result.Text);
            
            // Verify markdown formatting is removed
            Assert.DoesNotContain("**", result.Text);
            Assert.DoesNotContain("```", result.Text);
            Assert.DoesNotContain("[Link](", result.Text);
            Assert.Contains("bold", result.Text);
            Assert.Contains("italic", result.Text);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public async Task BookParserXceed_ParseNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var parser = new BookParser();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => parser.ParseAsync(nonExistentFile));
    }
    
    [Fact]
    public async Task BookParserXceed_ParseUnsupportedFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var parser = new BookParser();
        var tempFile = Path.GetTempFileName() + ".xyz";
        
        try
        {
            await File.WriteAllTextAsync(tempFile, "test content");
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => parser.ParseAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}

public class BookIndexingTests
{
    [Fact]
    public async Task BookIndexer_CreateIndex_ReturnsValidIndex()
    {
        // Arrange
        var indexer = new BookIndexer();
        var parseResult = new BookParseResult(
            Text: "This is the first sentence. This is the second sentence!\n\nThis is a new paragraph.",
            Title: "Test Book",
            Author: "Test Author"
        );
        var sourceFile = Path.GetTempFileName();
        var options = new BookIndexOptions();
        
        try
        {
            await File.WriteAllTextAsync(sourceFile, "dummy content");
            
            // Act
            var result = await indexer.CreateIndexAsync(parseResult, sourceFile, options);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Book", result.Title);
            Assert.Equal("Test Author", result.Author);
            Assert.Equal(sourceFile, result.SourceFile);
            Assert.True(result.TotalWords > 0);
            Assert.True(result.TotalSentences > 0);
            Assert.True(result.Words.Length > 0);
            Assert.True(result.Segments.Length > 0);
            Assert.True(result.EstimatedDuration > 0);
            
            // Verify word indexing
            var firstWord = result.Words[0];
            Assert.Equal(0, firstWord.WordIndex);
            Assert.Equal(0, firstWord.SentenceIndex);
            Assert.Equal(0, firstWord.ParagraphIndex);
            Assert.Null(firstWord.StartTime); // No timing initially
            
            // Verify segment indexing
            var sentenceSegments = result.Segments.Where(s => s.Type == BookSegmentType.Sentence).ToArray();
            Assert.True(sentenceSegments.Length >= 2); // At least two sentences
        }
        finally
        {
            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
        }
    }
    
    [Fact]
    public async Task BookIndexer_UpdateTiming_AddsTimingInformation()
    {
        // Arrange
        var indexer = new BookIndexer();
        var parseResult = new BookParseResult(
            Text: "Hello world. This is test."
        );
        var sourceFile = Path.GetTempFileName();
        
        try
        {
            await File.WriteAllTextAsync(sourceFile, "dummy");
            
            var bookIndex = await indexer.CreateIndexAsync(parseResult, sourceFile);
            
            var asrTokens = new[]
            {
                new AsrToken(0.0, 0.5, "hello"),
                new AsrToken(0.5, 0.5, "world"),
                new AsrToken(1.0, 0.5, "this"),
                new AsrToken(1.5, 0.5, "is"),
                new AsrToken(2.0, 0.5, "test")
            };
            
            // Act
            var result = await indexer.UpdateTimingAsync(bookIndex, asrTokens);
            
            // Assert
            var wordsWithTiming = result.Words.Where(w => w.StartTime.HasValue).ToArray();
            Assert.True(wordsWithTiming.Length > 0);
            
            // Check first word timing
            var firstWord = wordsWithTiming.FirstOrDefault(w => w.Text == "hello");
            Assert.NotNull(firstWord);
            Assert.Equal(0.0, firstWord.StartTime);
            Assert.Equal(0.5, firstWord.EndTime);
            
            // Check that segments also have timing
            var segmentsWithTiming = result.Segments.Where(s => s.StartTime.HasValue).ToArray();
            Assert.True(segmentsWithTiming.Length > 0);
        }
        finally
        {
            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
        }
    }
    
    [Fact]
    public async Task BookIndexer_CreateIndex_WithNullParseResult_ThrowsArgumentNullException()
    {
        // Arrange
        var indexer = new BookIndexer();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => indexer.CreateIndexAsync(null!, "test.txt"));
    }
    
    [Fact]
    public async Task BookIndexer_CreateIndex_WithEmptySourceFile_ThrowsArgumentException()
    {
        // Arrange
        var indexer = new BookIndexer();
        var parseResult = new BookParseResult(Text: "Test content");
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => indexer.CreateIndexAsync(parseResult, ""));
    }
}

public class BookCacheTests
{
    [Fact]
    public async Task BookCache_SetAndGet_WorksCorrectly()
    {
        // Arrange
        var tempCacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = new BookCache(tempCacheDir);
        var sourceFile = Path.GetTempFileName();
        
        try
        {
            await File.WriteAllTextAsync(sourceFile, "test content");
            
            var bookIndex = new BookIndex(
                SourceFile: sourceFile,
                SourceFileHash: "TESTHASH123",
                IndexedAt: DateTime.UtcNow,
                Title: "Test Book",
                Author: "Test Author",
                TotalWords: 10,
                TotalSentences: 2,
                TotalParagraphs: 1,
                EstimatedDuration: 60.0,
                Words: Array.Empty<BookWord>(),
                Segments: Array.Empty<BookSegment>()
            );
            
            // Act
            var setResult = await cache.SetAsync(bookIndex);
            var getResult = await cache.GetAsync(sourceFile);
            
            // Assert
            Assert.True(setResult);
            Assert.NotNull(getResult);
            Assert.Equal(bookIndex.SourceFile, getResult.SourceFile);
            Assert.Equal(bookIndex.Title, getResult.Title);
            Assert.Equal(bookIndex.Author, getResult.Author);
            Assert.Equal(bookIndex.TotalWords, getResult.TotalWords);
        }
        finally
        {
            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
            
            if (Directory.Exists(tempCacheDir))
                Directory.Delete(tempCacheDir, true);
        }
    }
    
    [Fact]
    public async Task BookCache_GetNonExistent_ReturnsNull()
    {
        // Arrange
        var tempCacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = new BookCache(tempCacheDir);
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        try
        {
            // Act
            var result = await cache.GetAsync(nonExistentFile);
            
            // Assert
            Assert.Null(result);
        }
        finally
        {
            if (Directory.Exists(tempCacheDir))
                Directory.Delete(tempCacheDir, true);
        }
    }
    
    [Fact]
    public async Task BookCache_Remove_RemovesEntry()
    {
        // Arrange
        var tempCacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = new BookCache(tempCacheDir);
        var sourceFile = Path.GetTempFileName();
        
        try
        {
            await File.WriteAllTextAsync(sourceFile, "test content");
            
            var bookIndex = new BookIndex(
                SourceFile: sourceFile,
                SourceFileHash: "TESTHASH123",
                IndexedAt: DateTime.UtcNow,
                Title: null,
                Author: null,
                TotalWords: 1,
                TotalSentences: 1,
                TotalParagraphs: 1,
                EstimatedDuration: 1.0,
                Words: Array.Empty<BookWord>(),
                Segments: Array.Empty<BookSegment>()
            );
            
            await cache.SetAsync(bookIndex);
            var beforeRemove = await cache.GetAsync(sourceFile);
            
            // Act
            var removeResult = await cache.RemoveAsync(sourceFile);
            var afterRemove = await cache.GetAsync(sourceFile);
            
            // Assert
            Assert.NotNull(beforeRemove);
            Assert.True(removeResult);
            Assert.Null(afterRemove);
        }
        finally
        {
            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
                
            if (Directory.Exists(tempCacheDir))
                Directory.Delete(tempCacheDir, true);
        }
    }
    
    [Fact]
    public async Task BookCache_Clear_RemovesAllEntries()
    {
        // Arrange
        var tempCacheDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var cache = new BookCache(tempCacheDir);
        var sourceFiles = new[]
        {
            Path.GetTempFileName(),
            Path.GetTempFileName()
        };
        
        try
        {
            // Add multiple cache entries
            foreach (var sourceFile in sourceFiles)
            {
                await File.WriteAllTextAsync(sourceFile, "test content");
                
                var bookIndex = new BookIndex(
                    SourceFile: sourceFile,
                    SourceFileHash: "HASH" + sourceFile.GetHashCode(),
                    IndexedAt: DateTime.UtcNow,
                    Title: null,
                    Author: null,
                    TotalWords: 1,
                    TotalSentences: 1,
                    TotalParagraphs: 1,
                    EstimatedDuration: 1.0,
                    Words: Array.Empty<BookWord>(),
                    Segments: Array.Empty<BookSegment>()
                );
                
                await cache.SetAsync(bookIndex);
            }
            
            // Verify entries exist
            foreach (var sourceFile in sourceFiles)
            {
                var cached = await cache.GetAsync(sourceFile);
                Assert.NotNull(cached);
            }
            
            // Act
            await cache.ClearAsync();
            
            // Assert
            foreach (var sourceFile in sourceFiles)
            {
                var cached = await cache.GetAsync(sourceFile);
                Assert.Null(cached);
            }
        }
        finally
        {
            foreach (var sourceFile in sourceFiles)
            {
                if (File.Exists(sourceFile))
                    File.Delete(sourceFile);
            }
            
            if (Directory.Exists(tempCacheDir))
                Directory.Delete(tempCacheDir, true);
        }
    }
}

public class BookModelsTests
{
    [Fact]
    public void BookWord_Initialization_WorksCorrectly()
    {
        // Arrange & Act
        var word = new BookWord(
            Text: "hello",
            WordIndex: 0,
            SentenceIndex: 0,
            ParagraphIndex: 0,
            StartTime: 1.0,
            EndTime: 2.0,
            Confidence: 0.95
        );
        
        // Assert
        Assert.Equal("hello", word.Text);
        Assert.Equal(0, word.WordIndex);
        Assert.Equal(0, word.SentenceIndex);
        Assert.Equal(0, word.ParagraphIndex);
        Assert.Equal(1.0, word.StartTime);
        Assert.Equal(2.0, word.EndTime);
        Assert.Equal(0.95, word.Confidence);
    }
    
    [Fact]
    public void BookSegment_Initialization_WorksCorrectly()
    {
        // Arrange & Act
        var segment = new BookSegment(
            Text: "This is a test sentence.",
            Type: BookSegmentType.Sentence,
            Index: 0,
            WordStartIndex: 0,
            WordEndIndex: 4,
            StartTime: 1.0,
            EndTime: 3.0,
            Confidence: 0.9
        );
        
        // Assert
        Assert.Equal("This is a test sentence.", segment.Text);
        Assert.Equal(BookSegmentType.Sentence, segment.Type);
        Assert.Equal(0, segment.Index);
        Assert.Equal(0, segment.WordStartIndex);
        Assert.Equal(4, segment.WordEndIndex);
        Assert.Equal(1.0, segment.StartTime);
        Assert.Equal(3.0, segment.EndTime);
        Assert.Equal(0.9, segment.Confidence);
    }
    
    [Fact]
    public void BookIndexOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new BookIndexOptions();
        
        // Assert
        Assert.Equal(200.0, options.AverageWpm);
        Assert.True(options.ExtractMetadata);
        Assert.True(options.NormalizeText);
        Assert.True(options.IncludeParagraphSegments);
        Assert.Equal(5, options.MinimumSentenceLength);
        Assert.Equal(2, options.MinimumParagraphWords);
    }
}