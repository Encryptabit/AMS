using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ams.Core;
using Ams.Core.Align.Tx;
using Ams.Core.Services;
using Xunit;

namespace Ams.Tests;

public sealed class HydratedTxServiceTests
{
    private readonly HydratedTxService _service = new();

    [Fact]
    public void GenerateHydratedTx_WithNullTranscriptIndex_ThrowsArgumentNullException()
    {
        // Arrange
        var book = CreateTestBookIndex();
        var asr = CreateTestAsrResponse();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _service.GenerateHydratedTx(null!, book, asr));
    }

    [Fact]
    public void GenerateHydratedTx_WithNullBookIndex_ThrowsArgumentNullException()
    {
        // Arrange
        var tx = CreateTestTranscriptIndex();
        var asr = CreateTestAsrResponse();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _service.GenerateHydratedTx(tx, null!, asr));
    }

    [Fact]
    public void GenerateHydratedTx_WithNullAsrResponse_ThrowsArgumentNullException()
    {
        // Arrange
        var tx = CreateTestTranscriptIndex();
        var book = CreateTestBookIndex();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _service.GenerateHydratedTx(tx, book, null!));
    }

    [Fact]
    public void GenerateHydratedTx_WithValidInput_PreservesMetadata()
    {
        // Arrange
        var tx = CreateTestTranscriptIndex();
        var book = CreateTestBookIndex();
        var asr = CreateTestAsrResponse();

        // Act
        var result = _service.GenerateHydratedTx(tx, book, asr);
        
        // Serialize to JSON and deserialize to validate structure
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var hydrated = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        Assert.Equal(tx.AudioPath, hydrated.GetProperty("audioPath").GetString());
        Assert.Equal(tx.ScriptPath, hydrated.GetProperty("scriptPath").GetString());
        Assert.Equal(tx.BookIndexPath, hydrated.GetProperty("bookIndexPath").GetString());
        Assert.Equal(tx.CreatedAtUtc, hydrated.GetProperty("createdAtUtc").GetDateTime());
        Assert.Equal(tx.NormalizationVersion, hydrated.GetProperty("normalizationVersion").GetString());
    }

    [Fact]
    public void GenerateHydratedTx_WithValidWords_HydratesWordAlignment()
    {
        // Arrange
        var bookWords = new[]
        {
            new BookWord("Hello", 0, 0, 0),
            new BookWord("world", 1, 0, 0)
        };
        var book = new BookIndex(
            "test.txt", "hash", DateTime.UtcNow, null, null,
            2, 1, 1, 1.0, bookWords, new BookSegment[0]);

        var asrTokens = new[]
        {
            new AsrToken(0.0, 0.5, "Hello"),
            new AsrToken(0.5, 0.5, "world")
        };
        var asr = new AsrResponse("test-model", asrTokens);

        var wordAligns = new[]
        {
            new WordAlign(0, 0, AlignOp.Match, "", 1.0),
            new WordAlign(1, 1, AlignOp.Match, "", 1.0)
        };

        var tx = new TranscriptIndex(
            "audio.wav", "script.txt", "book.json", DateTime.UtcNow, "v1",
            wordAligns, new SentenceAlign[0], new ParagraphAlign[0]);

        // Act
        var result = _service.GenerateHydratedTx(tx, book, asr);
        
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var hydrated = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        var words = hydrated.GetProperty("words").EnumerateArray().ToArray();
        Assert.Equal(2, words.Length);
        
        var firstWord = words[0];
        Assert.Equal(0, firstWord.GetProperty("bookIdx").GetInt32());
        Assert.Equal(0, firstWord.GetProperty("asrIdx").GetInt32());
        Assert.Equal("Hello", firstWord.GetProperty("bookWord").GetString());
        Assert.Equal("Hello", firstWord.GetProperty("asrWord").GetString());
        Assert.Equal("Match", firstWord.GetProperty("op").GetString());
        Assert.Equal("", firstWord.GetProperty("reason").GetString());
        Assert.Equal(1.0, firstWord.GetProperty("score").GetDouble());
    }

    [Fact]
    public void GenerateHydratedTx_WithAlignOpEnum_ConvertsToString()
    {
        // Arrange
        var wordAligns = new[]
        {
            new WordAlign(0, 0, AlignOp.Match, "", 1.0),
            new WordAlign(1, null, AlignOp.Del, "", 0.8),
            new WordAlign(null, 1, AlignOp.Ins, "", 0.9),
            new WordAlign(2, 2, AlignOp.Sub, "", 0.7)
        };

        var tx = new TranscriptIndex(
            "audio.wav", "script.txt", "book.json", DateTime.UtcNow, "v1",
            wordAligns, new SentenceAlign[0], new ParagraphAlign[0]);

        var book = CreateTestBookIndex();
        var asr = CreateTestAsrResponse();

        // Act
        var result = _service.GenerateHydratedTx(tx, book, asr);
        
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var hydrated = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        var words = hydrated.GetProperty("words").EnumerateArray().ToArray();
        Assert.Equal("Match", words[0].GetProperty("op").GetString());
        Assert.Equal("Del", words[1].GetProperty("op").GetString());
        Assert.Equal("Ins", words[2].GetProperty("op").GetString());
        Assert.Equal("Sub", words[3].GetProperty("op").GetString());
    }

    [Fact]
    public void GenerateHydratedTx_WithSentences_HydratesTextContent()
    {
        // Arrange
        var book = CreateTestBookIndex();
        var asr = CreateTestAsrResponse();

        var sentences = new[]
        {
            new SentenceAlign(1, new IntRange(0, 1), new ScriptRange(0, 1), 
                new SentenceMetrics(0.0, 0.0, 0.0, 0, 0), "Ok")
        };

        var tx = new TranscriptIndex(
            "audio.wav", "script.txt", "book.json", DateTime.UtcNow, "v1",
            new WordAlign[0], sentences, new ParagraphAlign[0]);

        // Act
        var result = _service.GenerateHydratedTx(tx, book, asr);
        
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var hydrated = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert - Basic functionality test
        Assert.True(hydrated.TryGetProperty("sentences", out var sentencesProperty));
        var hydratedSentences = sentencesProperty.EnumerateArray().ToArray();
        Assert.Single(hydratedSentences);
        
        var sentence = hydratedSentences[0];
        Assert.Equal(1, sentence.GetProperty("id").GetInt32());
    }

    [Fact]
    public void GenerateHydratedTx_WithParagraphs_HydratesBookText()
    {
        // Arrange
        var book = CreateTestBookIndex();
        var asr = CreateTestAsrResponse();

        var paragraphs = new[]
        {
            new ParagraphAlign(1, new IntRange(0, 1), new[] { 1 },
                new ParagraphMetrics(0.0, 0.0, 1.0), "Ok")
        };

        var tx = new TranscriptIndex(
            "audio.wav", "script.txt", "book.json", DateTime.UtcNow, "v1",
            new WordAlign[0], new SentenceAlign[0], paragraphs);

        // Act
        var result = _service.GenerateHydratedTx(tx, book, asr);
        
        // Assert - Just verify it doesn't crash and produces expected structure
        Assert.NotNull(result);
        var json = JsonSerializer.Serialize(result);
        Assert.Contains("paragraphs", json);
    }

    [Fact]
    public void GenerateHydratedTx_WithInvalidIndices_HandlesGracefully()
    {
        // Arrange
        var book = CreateTestBookIndex();
        var asr = CreateTestAsrResponse();

        // Word with out-of-bounds indices
        var wordAligns = new[]
        {
            new WordAlign(999, 999, AlignOp.Match, "", 1.0) // Out of bounds
        };

        var tx = new TranscriptIndex(
            "audio.wav", "script.txt", "book.json", DateTime.UtcNow, "v1",
            wordAligns, new SentenceAlign[0], new ParagraphAlign[0]);

        // Act & Assert - Should not throw exception
        var result = _service.GenerateHydratedTx(tx, book, asr);
        Assert.NotNull(result);
        var json = JsonSerializer.Serialize(result);
        Assert.Contains("words", json);
    }

    private TranscriptIndex CreateTestTranscriptIndex()
    {
        return new TranscriptIndex(
            "test-audio.wav", "test-script.txt", "test-book.json",
            DateTime.UtcNow, "v1.0",
            new WordAlign[0], new SentenceAlign[0], new ParagraphAlign[0]);
    }

    private BookIndex CreateTestBookIndex()
    {
        var words = new[]
        {
            new BookWord("Hello", 0, 0, 0),
            new BookWord("world", 1, 0, 0)
        };

        return new BookIndex(
            "test.txt", "hash123", DateTime.UtcNow, "Test Title", "Test Author",
            2, 1, 1, 5.0, words, new BookSegment[0]);
    }

    private AsrResponse CreateTestAsrResponse()
    {
        var tokens = new[]
        {
            new AsrToken(0.000000, 0.500000, "Hello"),
            new AsrToken(0.500000, 0.500000, "world")
        };

        return new AsrResponse("test-model-v1", tokens);
    }
}