using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core;
using Ams.Core.Services;
using Xunit;

namespace Ams.Tests;

public sealed class AsrRefinementServiceTests
{
    private readonly AsrRefinementService _service = new();

    [Fact]
    public void GenerateRefinedAsr_WithNullOriginalAsr_ThrowsArgumentNullException()
    {
        // Arrange
        var refinedSentences = new List<SentenceRefined>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _service.GenerateRefinedAsr(null!, refinedSentences));
    }

    [Fact]
    public void GenerateRefinedAsr_WithNullRefinedSentences_ThrowsArgumentNullException()
    {
        // Arrange
        var originalAsr = new AsrResponse("test-model", Array.Empty<AsrToken>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _service.GenerateRefinedAsr(originalAsr, null!));
    }

    [Fact]
    public void GenerateRefinedAsr_WithValidInput_PreservesModelVersion()
    {
        // Arrange
        const string expectedModelVersion = "nvidia/parakeet-tdt-0.6b-v3";
        var originalAsr = new AsrResponse(expectedModelVersion, new[]
        {
            new AsrToken(0.000000, 0.500000, "Hello"),
            new AsrToken(0.500000, 0.500000, "world")
        });
        
        var refinedSentences = new[]
        {
            new SentenceRefined(0.000000, 1.000000, 0, 1)
        };

        // Act
        var result = _service.GenerateRefinedAsr(originalAsr, refinedSentences);

        // Assert
        Assert.Equal(expectedModelVersion, result.ModelVersion);
    }

    [Fact]
    public void GenerateRefinedAsr_WithTokensWithinSentenceBounds_KeepsTokensUnchanged()
    {
        // Arrange
        var originalTokens = new[]
        {
            new AsrToken(1.000000, 0.500000, "Hello"),
            new AsrToken(1.500000, 0.500000, "world")
        };
        var originalAsr = new AsrResponse("test-model", originalTokens);
        
        var refinedSentences = new[]
        {
            new SentenceRefined(0.500000, 2.500000, 0, 1) // Sentence bounds contain all tokens
        };

        // Act
        var result = _service.GenerateRefinedAsr(originalAsr, refinedSentences);

        // Assert
        Assert.Equal(2, result.Tokens.Length);
        Assert.Equal(1.000000, result.Tokens[0].StartTime, 6);
        Assert.Equal(0.500000, result.Tokens[0].Duration, 6);
        Assert.Equal("Hello", result.Tokens[0].Word);
        Assert.Equal(1.500000, result.Tokens[1].StartTime, 6);
        Assert.Equal(0.500000, result.Tokens[1].Duration, 6);
        Assert.Equal("world", result.Tokens[1].Word);
    }

    [Fact]
    public void GenerateRefinedAsr_WithTokensOutsideSentenceBounds_DropsTokens()
    {
        // Arrange
        var originalTokens = new[]
        {
            new AsrToken(0.000000, 0.300000, "Before"), // Outside sentence (ends at 0.3)
            new AsrToken(1.000000, 0.500000, "Hello"),   // Inside sentence
            new AsrToken(3.000000, 0.500000, "After")    // Outside sentence (starts at 3.0)
        };
        var originalAsr = new AsrResponse("test-model", originalTokens);
        
        var refinedSentences = new[]
        {
            new SentenceRefined(0.500000, 2.500000, 1, 1) // Only middle token should be kept
        };

        // Act
        var result = _service.GenerateRefinedAsr(originalAsr, refinedSentences);

        // Assert
        Assert.Single(result.Tokens);
        Assert.Equal("Hello", result.Tokens[0].Word);
    }

    [Fact]
    public void GenerateRefinedAsr_WithTokenStraddlingBoundary_ClampsTokenToSentenceBounds()
    {
        // Arrange
        var originalTokens = new[]
        {
            new AsrToken(0.500000, 1.000000, "Straddling") // Token from 0.5 to 1.5, sentence ends at 1.2
        };
        var originalAsr = new AsrResponse("test-model", originalTokens);
        
        var refinedSentences = new[]
        {
            new SentenceRefined(0.000000, 1.200000, 0, 0) // Sentence clips token at 1.2
        };

        // Act
        var result = _service.GenerateRefinedAsr(originalAsr, refinedSentences);

        // Assert
        Assert.Single(result.Tokens);
        Assert.Equal(0.500000, result.Tokens[0].StartTime, 6);
        Assert.Equal(0.700000, result.Tokens[0].Duration, 6); // Clamped: 1.2 - 0.5 = 0.7
        Assert.Equal("Straddling", result.Tokens[0].Word);
    }

    [Fact]
    public void GenerateRefinedAsr_WithMicroFragment_DropsMicroFragment()
    {
        // Arrange
        var originalTokens = new[]
        {
            new AsrToken(1.000000, 0.002000, "Tiny") // 2ms token, sentence ends at 1.0005 (0.5ms fragment)
        };
        var originalAsr = new AsrResponse("test-model", originalTokens);
        
        var refinedSentences = new[]
        {
            new SentenceRefined(0.000000, 1.000500, 0, 0) // Creates 0.5ms fragment, below 1ms threshold
        };

        // Act
        var result = _service.GenerateRefinedAsr(originalAsr, refinedSentences);

        // Assert
        Assert.Empty(result.Tokens); // Micro-fragment should be dropped
    }

    [Fact]
    public void GenerateRefinedAsr_WithMultipleSentences_ProcessesInOrder()
    {
        // Arrange
        var originalTokens = new[]
        {
            new AsrToken(0.000000, 0.500000, "First"),  // Sentence 1
            new AsrToken(0.500000, 0.500000, "word"),   // Sentence 1
            new AsrToken(1.500000, 0.500000, "Second"), // Sentence 2
            new AsrToken(2.000000, 0.500000, "sentence") // Sentence 2
        };
        var originalAsr = new AsrResponse("test-model", originalTokens);
        
        var refinedSentences = new[]
        {
            new SentenceRefined(0.000000, 1.000000, 0, 1), // First sentence
            new SentenceRefined(1.200000, 2.500000, 2, 3)  // Second sentence
        };

        // Act
        var result = _service.GenerateRefinedAsr(originalAsr, refinedSentences);

        // Assert
        Assert.Equal(4, result.Tokens.Length);
        Assert.Equal("First", result.Tokens[0].Word);
        Assert.Equal("word", result.Tokens[1].Word);
        Assert.Equal("Second", result.Tokens[2].Word);
        Assert.Equal("sentence", result.Tokens[3].Word);
        
        // Verify monotonic ordering
        for (int i = 1; i < result.Tokens.Length; i++)
        {
            Assert.True(result.Tokens[i].StartTime >= result.Tokens[i-1].StartTime, 
                "Tokens should be in monotonic order");
        }
    }

    [Fact] 
    public void ValidateTimingConsistency_WithSimilarDurations_ReturnsTrue()
    {
        // Arrange
        var original = new AsrResponse("test", new[]
        {
            new AsrToken(0.0, 1.000000, "test")
        });
        var refined = new AsrResponse("test", new[]
        {
            new AsrToken(0.0, 0.999999, "test") // Within 1ms tolerance
        });

        // Act & Assert
        Assert.True(_service.ValidateTimingConsistency(original, refined));
    }

    [Fact]
    public void ValidateTimingConsistency_WithSignificantDifference_ReturnsFalse()
    {
        // Arrange
        var original = new AsrResponse("test", new[]
        {
            new AsrToken(0.0, 1.000000, "test")
        });
        var refined = new AsrResponse("test", new[]
        {
            new AsrToken(0.0, 0.995000, "test") // 5ms difference, above 1ms tolerance
        });

        // Act & Assert
        Assert.False(_service.ValidateTimingConsistency(original, refined));
    }
}