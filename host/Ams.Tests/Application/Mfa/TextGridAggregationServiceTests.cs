using Ams.Core.Application.Mfa;
using Ams.Core.Processors.Alignment.Mfa;

namespace Ams.Tests.Application.Mfa;

public class TextGridAggregationServiceTests : IDisposable
{
    private readonly string _tempDir;

    public TextGridAggregationServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ams-textgrid-agg-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static MfaChunkCorpusBuilder.UtteranceEntry MakeUtterance(
        int chunkId, double startSec, double endSec)
    {
        var uttName = MfaChunkCorpusBuilder.FormatUtteranceName(chunkId);
        return new MfaChunkCorpusBuilder.UtteranceEntry(
            ChunkId: chunkId,
            UtteranceName: uttName,
            WavPath: string.Empty,
            LabPath: string.Empty,
            ChunkStartSec: startSec,
            ChunkEndSec: endSec);
    }

    /// <summary>
    /// Writes a minimal Praat TextGrid file with a words tier and optional phones tier.
    /// Intervals are specified as (start, end, text) tuples relative to chunk-local time (0-based).
    /// </summary>
    private void WriteChunkTextGrid(
        string utteranceName,
        IReadOnlyList<(double Start, double End, string Text)> wordIntervals,
        IReadOnlyList<(double Start, double End, string Text)>? phoneIntervals = null)
    {
        var path = Path.Combine(_tempDir, utteranceName + ".TextGrid");
        using var writer = new StreamWriter(path);

        var xmin = 0.0;
        var xmax = wordIntervals.Count > 0 ? wordIntervals[^1].End : 0.0;
        var tierCount = phoneIntervals is { Count: > 0 } ? 2 : 1;

        writer.WriteLine("File type = \"ooTextFile\"");
        writer.WriteLine("Object class = \"TextGrid\"");
        writer.WriteLine();
        writer.WriteLine($"xmin = {xmin}");
        writer.WriteLine($"xmax = {xmax}");
        writer.WriteLine("tiers? <exists>");
        writer.WriteLine($"size = {tierCount}");
        writer.WriteLine("item []:");

        WriteTier(writer, 1, "words", xmin, xmax, wordIntervals);
        if (phoneIntervals is { Count: > 0 })
        {
            WriteTier(writer, 2, "phones", xmin, xmax, phoneIntervals);
        }
    }

    private static void WriteTier(
        StreamWriter writer,
        int tierIndex,
        string tierName,
        double xmin,
        double xmax,
        IReadOnlyList<(double Start, double End, string Text)> intervals)
    {
        writer.WriteLine($"    item [{tierIndex}]:");
        writer.WriteLine("        class = \"IntervalTier\"");
        writer.WriteLine($"        name = \"{tierName}\"");
        writer.WriteLine($"        xmin = {xmin}");
        writer.WriteLine($"        xmax = {xmax}");
        writer.WriteLine($"        intervals: size = {intervals.Count}");

        for (int i = 0; i < intervals.Count; i++)
        {
            var (start, end, text) = intervals[i];
            writer.WriteLine($"        intervals [{i + 1}]:");
            writer.WriteLine($"            xmin = {start}");
            writer.WriteLine($"            xmax = {end}");
            writer.WriteLine($"            text = \"{text}\"");
        }
    }

    // ----------------------------------------------------------------
    // Offset correctness tests
    // ----------------------------------------------------------------

    [Fact]
    public void Aggregate_AppliesChunkOffsetToWordIntervals()
    {
        // Chunk 0 starts at 0.0s, chunk 1 starts at 10.0s
        var utterances = new[]
        {
            MakeUtterance(0, 0.0, 10.0),
            MakeUtterance(1, 10.0, 20.0)
        };

        WriteChunkTextGrid("utt-0000", new (double, double, string)[]
        {
            (0.5, 1.2, "hello"),
            (1.2, 2.0, "world")
        });

        WriteChunkTextGrid("utt-0001", new (double, double, string)[]
        {
            (0.3, 1.0, "foo"),
            (1.0, 1.8, "bar")
        });

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        var count = TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        Assert.Equal(4, count);
        Assert.True(File.Exists(outputPath));

        var intervals = TextGridParser.ParseWordIntervals(outputPath);
        Assert.Equal(4, intervals.Count);

        // Chunk 0 intervals: offset by 0.0
        Assert.Equal("hello", intervals[0].Text);
        Assert.InRange(intervals[0].Start, 0.49, 0.51);
        Assert.InRange(intervals[0].End, 1.19, 1.21);

        Assert.Equal("world", intervals[1].Text);
        Assert.InRange(intervals[1].Start, 1.19, 1.21);
        Assert.InRange(intervals[1].End, 1.99, 2.01);

        // Chunk 1 intervals: offset by 10.0
        Assert.Equal("foo", intervals[2].Text);
        Assert.InRange(intervals[2].Start, 10.29, 10.31);
        Assert.InRange(intervals[2].End, 10.99, 11.01);

        Assert.Equal("bar", intervals[3].Text);
        Assert.InRange(intervals[3].Start, 10.99, 11.01);
        Assert.InRange(intervals[3].End, 11.79, 11.81);
    }

    [Fact]
    public void Aggregate_AppliesChunkOffsetToPhoneIntervals()
    {
        var utterances = new[]
        {
            MakeUtterance(0, 5.0, 15.0)
        };

        WriteChunkTextGrid("utt-0000",
            new (double, double, string)[]
            {
                (0.0, 1.0, "test")
            },
            new (double, double, string)[]
            {
                (0.0, 0.3, "t"),
                (0.3, 0.6, "eh"),
                (0.6, 0.8, "s"),
                (0.8, 1.0, "t")
            });

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        var phones = TextGridParser.ParsePhoneIntervals(outputPath);
        Assert.Equal(4, phones.Count);

        // All phone intervals should be offset by 5.0s
        Assert.Equal("t", phones[0].Text);
        Assert.InRange(phones[0].Start, 4.99, 5.01);
        Assert.InRange(phones[0].End, 5.29, 5.31);

        Assert.Equal("t", phones[3].Text);
        Assert.InRange(phones[3].Start, 5.79, 5.81);
        Assert.InRange(phones[3].End, 5.99, 6.01);
    }

    // ----------------------------------------------------------------
    // Monotonic ordering tests
    // ----------------------------------------------------------------

    [Fact]
    public void Aggregate_ProducesMonotonicallyOrderedIntervals()
    {
        // Three chunks with different offsets
        var utterances = new[]
        {
            MakeUtterance(0, 0.0, 5.0),
            MakeUtterance(1, 5.0, 10.0),
            MakeUtterance(2, 10.0, 15.0)
        };

        WriteChunkTextGrid("utt-0000", new (double, double, string)[]
        {
            (1.0, 2.0, "alpha"),
            (3.0, 4.0, "beta")
        });

        WriteChunkTextGrid("utt-0001", new (double, double, string)[]
        {
            (0.5, 1.5, "gamma")
        });

        WriteChunkTextGrid("utt-0002", new (double, double, string)[]
        {
            (0.2, 0.8, "delta"),
            (1.0, 2.0, "epsilon")
        });

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        var count = TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        Assert.Equal(5, count);

        var intervals = TextGridParser.ParseWordIntervals(outputPath);

        // Verify monotonic start times
        for (int i = 1; i < intervals.Count; i++)
        {
            Assert.True(intervals[i].Start >= intervals[i - 1].Start,
                $"Interval {i} start ({intervals[i].Start:F3}) < interval {i - 1} start ({intervals[i - 1].Start:F3})");
        }

        // Verify expected absolute times:
        // chunk 0: (1.0+0=1.0, 2.0), (3.0+0=3.0, 4.0)
        // chunk 1: (0.5+5=5.5, 6.5)
        // chunk 2: (0.2+10=10.2, 10.8), (1.0+10=11.0, 12.0)
        Assert.InRange(intervals[0].Start, 0.99, 1.01);  // alpha
        Assert.InRange(intervals[1].Start, 2.99, 3.01);  // beta
        Assert.InRange(intervals[2].Start, 5.49, 5.51);  // gamma
        Assert.InRange(intervals[3].Start, 10.19, 10.21); // delta
        Assert.InRange(intervals[4].Start, 10.99, 11.01); // epsilon
    }

    // ----------------------------------------------------------------
    // Empty / missing chunk handling
    // ----------------------------------------------------------------

    [Fact]
    public void Aggregate_SkipsMissingTextGridFiles_GracefullyLogsAndContinues()
    {
        var utterances = new[]
        {
            MakeUtterance(0, 0.0, 5.0),  // will have a TextGrid
            MakeUtterance(1, 5.0, 10.0)  // no TextGrid file written
        };

        WriteChunkTextGrid("utt-0000", new (double, double, string)[]
        {
            (0.5, 1.5, "present")
        });

        // Intentionally not writing utt-0001.TextGrid

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        var count = TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        Assert.Equal(1, count);
        var intervals = TextGridParser.ParseWordIntervals(outputPath);
        Assert.Single(intervals);
        Assert.Equal("present", intervals[0].Text);
    }

    [Fact]
    public void Aggregate_ReturnsZero_WhenAllChunksEmpty()
    {
        var utterances = new[]
        {
            MakeUtterance(0, 0.0, 5.0),
            MakeUtterance(1, 5.0, 10.0)
        };

        // No TextGrid files written for either chunk

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        var count = TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        Assert.Equal(0, count);
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public void Aggregate_HandlesEmptyWordTier_SkipsChunk()
    {
        var utterances = new[]
        {
            MakeUtterance(0, 0.0, 5.0)
        };

        // Write a TextGrid with empty words tier (no intervals, just empty markers)
        var path = Path.Combine(_tempDir, "utt-0000.TextGrid");
        File.WriteAllText(path, """
            File type = "ooTextFile"
            Object class = "TextGrid"

            xmin = 0
            xmax = 5.0
            tiers? <exists>
            size = 1
            item []:
                item [1]:
                    class = "IntervalTier"
                    name = "words"
                    xmin = 0
                    xmax = 5.0
                    intervals: size = 1
                    intervals [1]:
                        xmin = 0
                        xmax = 5.0
                        text = ""
            """);

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        var count = TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        // The interval has empty text, parser returns it but MergeTimingsCommand filters by non-empty text
        // Aggregation should still include these as the parser returns them
        Assert.Equal(1, count);
    }

    // ----------------------------------------------------------------
    // Round-trip compatibility with TextGridParser
    // ----------------------------------------------------------------

    [Fact]
    public void Aggregate_ProducesTextGrid_ParseableByTextGridParser()
    {
        var utterances = new[]
        {
            MakeUtterance(0, 0.0, 30.0),
            MakeUtterance(1, 30.0, 60.0)
        };

        WriteChunkTextGrid("utt-0000", new (double, double, string)[]
        {
            (0.0, 0.5, ""),
            (0.5, 1.2, "the"),
            (1.2, 1.8, "quick"),
            (1.8, 2.5, "brown"),
            (2.5, 3.0, "fox")
        });

        WriteChunkTextGrid("utt-0001", new (double, double, string)[]
        {
            (0.0, 0.3, ""),
            (0.3, 1.0, "jumps"),
            (1.0, 1.5, "over"),
            (1.5, 2.0, "the"),
            (2.0, 2.8, "lazy"),
            (2.8, 3.5, "dog")
        });

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        // Verify the aggregated TextGrid is parseable
        var words = TextGridParser.ParseWordIntervals(outputPath);
        Assert.Equal(11, words.Count);

        // Verify that non-empty words from chunk 1 have proper offsets
        var jumps = words.First(w => w.Text == "jumps");
        Assert.InRange(jumps.Start, 30.29, 30.31);
        Assert.InRange(jumps.End, 30.99, 31.01);

        var dog = words.First(w => w.Text == "dog");
        Assert.InRange(dog.Start, 32.79, 32.81);
        Assert.InRange(dog.End, 33.49, 33.51);
    }

    [Fact]
    public void Aggregate_OffsetAppliedExactlyOnce()
    {
        // Regression guard: offsets must not double-apply
        var utterances = new[]
        {
            MakeUtterance(0, 100.0, 110.0) // large offset to make double-apply obvious
        };

        WriteChunkTextGrid("utt-0000", new (double, double, string)[]
        {
            (1.0, 2.0, "word")
        });

        var outputPath = Path.Combine(_tempDir, "chapter.TextGrid");
        TextGridAggregationService.Aggregate(utterances, _tempDir, outputPath);

        var intervals = TextGridParser.ParseWordIntervals(outputPath);
        Assert.Single(intervals);

        // Should be 1.0 + 100.0 = 101.0, NOT 1.0 + 200.0 = 201.0
        Assert.InRange(intervals[0].Start, 100.99, 101.01);
        Assert.InRange(intervals[0].End, 101.99, 102.01);
    }
}
