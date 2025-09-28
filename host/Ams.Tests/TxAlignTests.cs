using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core.Artifacts;
using Ams.Core.Alignment.Tx;
using Ams.Core.Book;
using Ams.Core.Asr;
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
            new WordAlign(109, 9, AlignOp.Match, "anchor", 0),
            new WordAlign(null, 50, AlignOp.Ins, "extra", 1),
            new WordAlign(110, 10, AlignOp.Match, "equal_or_equiv", 0),
            new WordAlign(111, 11, AlignOp.Match, "equal_or_equiv", 0),
            new WordAlign(112, 12, AlignOp.Sub, "near_or_diff", 1),
            new WordAlign(113, 13, AlignOp.Match, "equal_or_equiv", 0),
            new WordAlign(120, 20, AlignOp.Match, "anchor", 0)
        };

        var words = Enumerable.Range(0, 130)
            .Select(i => new BookWord($"w{i}", i, i >= 110 && i <= 113 ? 42 : 0, 0))
            .ToArray();
        var sentencesMeta = new[] { new SentenceRange(42, 110, 113) };
        var paragraphsMeta = new[] { new ParagraphRange(9, 110, 113, "Body", "Style") };
        var book = new BookIndex(
            SourceFile: "book",
            SourceFileHash: "hash",
            IndexedAt: DateTime.UtcNow,
            Title: null,
            Author: null,
            Totals: new BookTotals(words.Length, sentencesMeta.Length, paragraphsMeta.Length, 0),
            Words: words,
            Sentences: sentencesMeta,
            Paragraphs: paragraphsMeta,
            Sections: Array.Empty<SectionRange>());

        var tokens = Enumerable.Range(0, 200)
            .Select(i => new AsrToken(i * 0.1, 0.1, $"a{i}"))
            .ToArray();
        var asr = new AsrResponse("model", tokens);

        var (sentences, _) = TranscriptAligner.Rollup(
            ops,
            new List<(int Id, int Start, int End)> { (42, 110, 113) },
            new List<(int Id, int Start, int End)> { (9, 110, 113) },
            book,
            asr);

        var metrics = Assert.Single(sentences).Metrics;

        Assert.Equal(0, metrics.ExtraRuns);
        Assert.Equal(0.25, metrics.Wer);
        Assert.Equal(0, metrics.MissingRuns);
    }

    [Fact]
    public void Rollup_UsesWeightedWerAndCer()
    {
        var ops = new List<WordAlign>
        {
            new WordAlign(0, 0, AlignOp.Sub, "near_or_diff", 0.3)
        };

        var bookWords = new[] { new BookWord("hello", 0, 0, 0) };
        var sentences = new[] { new SentenceRange(0, 0, 0) };
        var paragraphs = new[] { new ParagraphRange(0, 0, 0, "Body", "Style") };
        var book = new BookIndex(
            SourceFile: "book",
            SourceFileHash: "hash",
            IndexedAt: DateTime.UtcNow,
            Title: null,
            Author: null,
            Totals: new BookTotals(bookWords.Length, sentences.Length, paragraphs.Length, 0),
            Words: bookWords,
            Sentences: sentences,
            Paragraphs: paragraphs,
            Sections: Array.Empty<SectionRange>());

        var tokens = new[] { new AsrToken(0, 0.2, "hxllo") };
        var asr = new AsrResponse("model", tokens);

        var (sentAlign, _) = TranscriptAligner.Rollup(
            ops,
            new List<(int Id, int Start, int End)> { (0, 0, 0) },
            new List<(int Id, int Start, int End)> { (0, 0, 0) },
            book,
            asr);

        var metrics = Assert.Single(sentAlign).Metrics;
        Assert.InRange(metrics.Wer, 0.29, 0.31); // weighted cost 0.3
        Assert.InRange(metrics.Cer, 0.19, 0.21); // character distance 1/5
        Assert.InRange(metrics.SpanWer, 0.99, 1.01); // legacy WER remains 1.0
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
