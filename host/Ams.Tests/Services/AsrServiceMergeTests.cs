using Ams.Core.Services;

namespace Ams.Tests.Services;

/// <summary>
/// Tests for <see cref="AsrService.MergeChunkResponses"/> verifying
/// plan-driven multi-chunk merge semantics, monotonic timestamp ordering,
/// and non-overlap duplicate prevention at chunk boundaries.
/// </summary>
public class AsrServiceMergeTests
{
    [Fact]
    public void MergeChunkResponses_SingleChunk_PreservesTimestamps()
    {
        var response = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "hello"),
            new AsrToken(0.5, 0.3, "world")
        }, new[]
        {
            new AsrSegment(0.0, 0.8, "hello world")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (response, 0.0)
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(2, merged.Tokens.Length);
        Assert.Equal(0.0, merged.Tokens[0].StartTime);
        Assert.Equal(0.5, merged.Tokens[1].StartTime);
        Assert.Single(merged.Segments);
        Assert.Equal(0.0, merged.Segments[0].StartSec);
        Assert.Equal(0.8, merged.Segments[0].EndSec);
    }

    [Fact]
    public void MergeChunkResponses_MultiChunk_AppliesOffsetsCorrectly()
    {
        var chunk1 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "hello"),
            new AsrToken(0.5, 0.3, "world")
        }, new[]
        {
            new AsrSegment(0.0, 0.8, "hello world")
        });

        var chunk2 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.4, "foo"),
            new AsrToken(0.4, 0.3, "bar")
        }, new[]
        {
            new AsrSegment(0.0, 0.7, "foo bar")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk1, 0.0),
            (chunk2, 60.0)
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(4, merged.Tokens.Length);
        // Chunk 1 tokens at original timestamps
        Assert.Equal(0.0, merged.Tokens[0].StartTime);
        Assert.Equal(0.5, merged.Tokens[1].StartTime);
        // Chunk 2 tokens offset by 60.0
        Assert.Equal(60.0, merged.Tokens[2].StartTime);
        Assert.Equal(60.4, merged.Tokens[3].StartTime);

        Assert.Equal(2, merged.Segments.Length);
        Assert.Equal(0.0, merged.Segments[0].StartSec);
        Assert.Equal(0.8, merged.Segments[0].EndSec);
        Assert.Equal(60.0, merged.Segments[1].StartSec);
        Assert.Equal(60.7, merged.Segments[1].EndSec);
    }

    [Fact]
    public void MergeChunkResponses_TokenTimestampsAreMonotonicallyNonDecreasing()
    {
        // Simulate a boundary overlap scenario where chunk2's first token
        // after offset would have a timestamp before chunk1's last token end
        var chunk1 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "hello"),
            new AsrToken(0.5, 1.0, "world") // ends at 1.5
        });

        var chunk2 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.4, "foo") // after 1.0 offset: starts at 1.0, which is < 1.5
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk1, 0.0),
            (chunk2, 1.0) // intentionally tight offset to test clamping
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        // Verify monotonic non-decreasing token timestamps
        for (int i = 1; i < merged.Tokens.Length; i++)
        {
            Assert.True(merged.Tokens[i].StartTime >= merged.Tokens[i - 1].StartTime,
                $"Token {i} start ({merged.Tokens[i].StartTime:F3}) should be >= " +
                $"token {i - 1} start ({merged.Tokens[i - 1].StartTime:F3})");
        }

        // The clamped token should be at high-water mark (1.5), not raw 1.0
        Assert.True(merged.Tokens[2].StartTime >= 1.5,
            $"Clamped token start ({merged.Tokens[2].StartTime:F3}) should be >= 1.5");
    }

    [Fact]
    public void MergeChunkResponses_SegmentTimestampsAreMonotonicallyNonDecreasing()
    {
        var chunk1 = new AsrResponse("whisper-v1", segments: new[]
        {
            new AsrSegment(0.0, 10.0, "first segment")
        });

        var chunk2 = new AsrResponse("whisper-v1", segments: new[]
        {
            new AsrSegment(0.0, 5.0, "second segment") // after 9.0 offset: 9.0-14.0
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk1, 0.0),
            (chunk2, 9.0) // overlaps with chunk1's end
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(2, merged.Segments.Length);
        // Second segment start clamped to at least chunk1's end
        Assert.True(merged.Segments[1].StartSec >= merged.Segments[0].EndSec,
            $"Segment 1 start ({merged.Segments[1].StartSec:F3}) should be >= " +
            $"segment 0 end ({merged.Segments[0].EndSec:F3})");
    }

    [Fact]
    public void MergeChunkResponses_DeterministicOrdering_ChunksProcessedByOffset()
    {
        // Supply chunks in reverse offset order to verify sorting
        var chunk1 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "first")
        });

        var chunk2 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "second")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk2, 60.0), // later chunk listed first
            (chunk1, 0.0)   // earlier chunk listed second
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(2, merged.Tokens.Length);
        Assert.Equal("first", merged.Tokens[0].Word);
        Assert.Equal("second", merged.Tokens[1].Word);
        Assert.True(merged.Tokens[0].StartTime < merged.Tokens[1].StartTime);
    }

    [Fact]
    public void MergeChunkResponses_PreservesModelVersion()
    {
        var chunk1 = new AsrResponse("ggml-base.en.bin", new[]
        {
            new AsrToken(0.0, 0.5, "hello")
        });

        var chunk2 = new AsrResponse("ggml-base.en.bin", new[]
        {
            new AsrToken(0.0, 0.5, "world")
        });

        var merged = AsrService.MergeChunkResponses(new List<(AsrResponse, double)>
        {
            (chunk1, 0.0),
            (chunk2, 30.0)
        });

        Assert.Equal("ggml-base.en.bin", merged.ModelVersion);
    }

    [Fact]
    public void MergeChunkResponses_EmptyChunks_ReturnsDefaultResponse()
    {
        var merged = AsrService.MergeChunkResponses(
            Array.Empty<(AsrResponse Response, double OffsetSec)>());

        Assert.Equal("whisper", merged.ModelVersion);
        Assert.Empty(merged.Tokens);
        Assert.Empty(merged.Segments);
    }

    [Fact]
    public void MergeChunkResponses_ChunkWithNoTokens_HandledGracefully()
    {
        var chunk1 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "hello")
        });

        // Empty response (e.g., silence chunk with no speech)
        var chunk2 = new AsrResponse("whisper-v1",
            Array.Empty<AsrToken>(),
            Array.Empty<AsrSegment>());

        var chunk3 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "world")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk1, 0.0),
            (chunk2, 30.0),
            (chunk3, 60.0)
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(2, merged.Tokens.Length);
        Assert.Equal("hello", merged.Tokens[0].Word);
        Assert.Equal("world", merged.Tokens[1].Word);
        Assert.Equal(60.0, merged.Tokens[1].StartTime);
    }

    [Fact]
    public void MergeChunkResponses_NoDuplicateOffsetApplication()
    {
        // Verify the offset is applied exactly once, not compounded
        var response = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(1.0, 0.5, "test")
        }, new[]
        {
            new AsrSegment(1.0, 1.5, "test")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (response, 10.0)
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(11.0, merged.Tokens[0].StartTime); // 1.0 + 10.0 = 11.0, not 21.0
        Assert.Equal(11.0, merged.Segments[0].StartSec);
        Assert.Equal(11.5, merged.Segments[0].EndSec);
    }

    [Fact]
    public void MergeChunkResponses_ThreeChunks_AllTokensMonotonic()
    {
        var chunk1 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "a"),
            new AsrToken(0.5, 0.5, "b"),
            new AsrToken(1.0, 0.5, "c")
        });

        var chunk2 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "d"),
            new AsrToken(0.5, 0.5, "e")
        });

        var chunk3 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "f"),
            new AsrToken(0.5, 0.5, "g"),
            new AsrToken(1.0, 0.5, "h")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk1, 0.0),
            (chunk2, 30.0),
            (chunk3, 60.0)
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(8, merged.Tokens.Length);
        for (int i = 1; i < merged.Tokens.Length; i++)
        {
            Assert.True(merged.Tokens[i].StartTime >= merged.Tokens[i - 1].StartTime,
                $"Token {i} ('{merged.Tokens[i].Word}' at {merged.Tokens[i].StartTime:F3}) " +
                $"should be >= token {i - 1} ('{merged.Tokens[i - 1].Word}' at {merged.Tokens[i - 1].StartTime:F3})");
        }

        // Verify words are in correct order
        Assert.Equal("a", merged.Tokens[0].Word);
        Assert.Equal("h", merged.Tokens[7].Word);
    }

    [Fact]
    public void MergeChunkResponses_PreservesTokenDuration()
    {
        var chunk1 = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.75, "hello")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk1, 10.0)
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal(0.75, merged.Tokens[0].Duration);
    }

    [Fact]
    public void MergeChunkResponses_PreservesSegmentText()
    {
        var chunk1 = new AsrResponse("whisper-v1", segments: new[]
        {
            new AsrSegment(0.0, 5.0, "Hello world, this is a test.")
        });

        var chunks = new List<(AsrResponse Response, double OffsetSec)>
        {
            (chunk1, 0.0)
        };

        var merged = AsrService.MergeChunkResponses(chunks);

        Assert.Equal("Hello world, this is a test.", merged.Segments[0].Text);
    }

    [Fact]
    public void MergeChunkResponses_OutputShapeMatchesAsrResponseContract()
    {
        var chunk = new AsrResponse("whisper-v1", new[]
        {
            new AsrToken(0.0, 0.5, "hello")
        }, new[]
        {
            new AsrSegment(0.0, 0.5, "hello")
        });

        var merged = AsrService.MergeChunkResponses(new List<(AsrResponse, double)>
        {
            (chunk, 0.0)
        });

        // Verify the merged response maintains the same contract
        Assert.NotNull(merged.ModelVersion);
        Assert.NotNull(merged.Tokens);
        Assert.NotNull(merged.Segments);
        Assert.True(merged.HasWords);
        Assert.Equal(1, merged.WordCount);
        Assert.Equal("hello", merged.GetWord(0));
    }
}
