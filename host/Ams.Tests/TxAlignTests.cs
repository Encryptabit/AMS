using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts;
using Ams.Core.Asr;
using Ams.Core.Book;
using Ams.Core.Alignment.Tx;
using Xunit;

public class TxAlignTests
{
    [Fact]
    public void WindowsFromAnchors_AreClampedAndHalfOpen()
    {
        var anchors = new List<(int, int)> { (105, 5), (112, 14) };
        var wins = WindowBuilder.Build(anchors, bookStart: 100, bookEnd: 119, asrStart: 0, asrEnd: 24);
        Assert.Collection(wins,
            w => Assert.Equal((100, 105, 0, 5), w),
            w => Assert.Equal((106, 112, 6, 14), w),
            w => Assert.Equal((113, 120, 15, 25), w));
    }
    [Fact]
    public void Rollup_IgnoresInsertionsOutsideGuardSpan()
    {
        var ops = new List<WordAlign>
        {
            new WordAlign(109, 1000, AlignOp.Match, "anchor", 1),
            new WordAlign(null, 200, AlignOp.Ins, "extra", 0),
            new WordAlign(110, 1100, AlignOp.Match, "equal_or_equiv", 1),
            new WordAlign(111, 1101, AlignOp.Match, "equal_or_equiv", 1),
            new WordAlign(112, 1102, AlignOp.Sub, "near_or_diff", 0),
            new WordAlign(113, 1103, AlignOp.Match, "equal_or_equiv", 1),
            new WordAlign(120, 1200, AlignOp.Match, "anchor", 1)
        };

        var bookWords = Enumerable.Range(0, 1500)
            .Select(i => new BookWord($"word{i}", i, 0, 0, 0))
            .ToArray();
        var asrTokens = Enumerable.Range(0, 1500)
            .Select(i => new AsrToken(i * 0.1, 0.1, $"token{i}"))
            .ToArray();

        var (sentences, _) = TranscriptAligner.Rollup(
            ops,
            new List<(int Id, int Start, int End)> { (42, 110, 113) },
            new List<(int Id, int Start, int End)> { (9, 110, 113) },
            bookWords,
            asrTokens);

        var metrics = Assert.Single(sentences).Metrics;

        Assert.Equal(0, metrics.ExtraRuns);
        Assert.Equal(0.25, metrics.Wer);
        Assert.Equal(0, metrics.MissingRuns);
    }

    [Fact]
    public void Rollup_NormalizesCompoundWords()
    {
        var ops = new List<WordAlign>
        {
            new WordAlign(0, 0, AlignOp.Match, "anchor", 1),
            new WordAlign(null, 1, AlignOp.Ins, "extra", 0),
            new WordAlign(null, 2, AlignOp.Ins, "extra", 0),
            new WordAlign(1, null, AlignOp.Del, "missing_book", 0),
            new WordAlign(2, 3, AlignOp.Match, "anchor", 1)
        };

        var bookWords = new[]
        {
            new BookWord("The", 0, 0, 0, 0),
            new BookWord("dropship", 1, 0, 0, 0),
            new BookWord("arrived", 2, 0, 0, 0)
        };

        var asrTokens = new[]
        {
            new AsrToken(0.0, 0.2, "The"),
            new AsrToken(0.2, 0.2, "drop"),
            new AsrToken(0.4, 0.2, "ship"),
            new AsrToken(0.6, 0.2, "arrived")
        };

        var (sentences, _) = TranscriptAligner.Rollup(
            ops,
            new List<(int Id, int Start, int End)> { (1, 0, 2) },
            new List<(int Id, int Start, int End)> { (1, 0, 2) },
            bookWords,
            asrTokens);

        var sentence = Assert.Single(sentences);
        Assert.Equal("ok", sentence.Status);
        Assert.Equal(0.0, sentence.Metrics.Wer);
        Assert.Equal(0, sentence.Metrics.MissingRuns);
        Assert.Equal(0, sentence.Metrics.ExtraRuns);
    }
    [Fact]
    public void Align_SimpleNearMatch_YieldsSubNotDelIns()
    {
        var book = new[] { "the", "black", "forest", "was", "dark" };
        var asr =  new[] { "the", "black", "forest", "felt", "dark" };
        var anchors = new List<(int,int)> { (0,0) }; // pin start
        var wins = WindowBuilder.Build(anchors, 0, book.Length-1, 0, asr.Length-1);

        var ops = TranscriptAligner.AlignWindows(book, asr, wins,
            new Dictionary<string,string>(), new HashSet<string>());

        Assert.Equal(3, ops.Count(o => o.op == AlignOp.Match));
        Assert.Equal(1, ops.Count(o => o.op == AlignOp.Sub));
        Assert.Equal(0, ops.Count(o => o.op == AlignOp.Del));
        Assert.Equal(0, ops.Count(o => o.op == AlignOp.Ins));
    }
}
