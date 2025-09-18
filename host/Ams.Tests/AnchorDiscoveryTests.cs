using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Align.Anchors;
using Ams.Core;
using Ams.Core.Align;
using Ams.Core.Align.Anchors;
using Xunit;

public class AnchorDiscoveryTests
{
    [Fact]
    public void UniqueTrigrams_ProduceAnchors()
    {
        var book = new List<string> { "the","black","forest","was","dark" };
        var asr  = new List<string> { "the","black","forest","felt","dark" };
        var sent = new List<int>    { 0,0,0,0,0 };

        var policy = new AnchorPolicy(NGram:3, TargetPerTokens:50,
            Stopwords: new HashSet<string>{"the","was","felt"}, DisallowBoundaryCross:true);

        var anchors = AnchorDiscovery.SelectAnchors(book, sent, asr, policy);
        Assert.Contains(anchors, a => a.Bp == 1 && a.Ap == 1);
    }

    [Fact]
    public void LisEnforcesMonotonicity()
    {
        var lis = AnchorDiscovery.LisByAp(new List<(int,int)>{(10,50),(20,40),(30,60)});
        Assert.Equal(2, lis.Count);
        Assert.True(lis[0].Item2 < lis[1].Item2);
    }

    [Fact]
    public void WindowsAreClampedWithSentinels()
    {
        var anchors = new List<Anchor> { new(10,20), new(30,40) };
        var wins = AnchorDiscovery.BuildWindows(anchors, bookStart:0, bookEnd:49, asrStart:0, asrEnd:59);
        Assert.NotEmpty(wins);
        Assert.True(wins[0].bLo == 0 && wins[0].bHi == 10);
    }
}

public class SectionLocatorTests
{
    private static BookIndex MakeBookIndex()
    {
        var words = new List<BookWord>();
        for (int i = 0; i < 100; i++)
        {
            words.Add(new BookWord($"w{i}", i, i / 5, i / 10, 0));
        }

        var sections = new[]
        {
            new SectionRange(0, "Prologue", 1, "Heading", 0, 29, 0, 2),
            new SectionRange(1, "Chapter 14: Storm", 1, "Heading", 30, 99, 3, 9)
        };

        var sentences = Enumerable.Range(0, 20)
            .Select(i =>
            {
                int start = i * 5;
                int end = Math.Min(start + 4, words.Count - 1);
                return new BookSentence(i, start, end);
            })
            .ToArray();

        var paragraphs = Enumerable.Range(0, 10)
            .Select(i =>
            {
                int start = i * 10;
                int end = Math.Min(start + 9, words.Count - 1);
                return new BookParagraph(i, start, end, "Body", "Normal");
            })
            .ToArray();

        var totals = new BookTotals(
            Words: words.Count,
            Sentences: sentences.Length,
            Paragraphs: paragraphs.Length,
            EstimatedDurationSec: 30.0
        );

        return new BookIndex(
            SourceFile: "fake.docx",
            SourceFileHash: "hash",
            IndexedAt: DateTime.UtcNow,
            Title: "Test",
            Author: "Author",
            Totals: totals,
            Words: words.ToArray(),
            Sentences: sentences,
            Paragraphs: paragraphs,
            Sections: sections,
            BuildWarnings: Array.Empty<string>()
        );
    }

    [Fact]
    public void Detects_Chapter_From_Asr_Prefix()
    {
        var book = MakeBookIndex();
        var asr = new List<string> { "chapter", "14" , "storm" };
        var sec = SectionLocator.DetectSection(book, asr);
        Assert.NotNull(sec);
        Assert.Equal(1, sec!.Id);
    }

    [Fact]
    public void AnchorSelection_Respects_Book_Window()
    {
        var book = new List<string> { "a","b","c","d","e","f","g","h","i","j",
                                      "k","l","black","forest","m","n","o" };
        var sent = Enumerable.Repeat(0, book.Count).ToList();
        var asr  = new List<string> { "x","black","forest","y" };

        var policy = new AnchorPolicy(NGram:2, TargetPerTokens:50,
            Stopwords: new HashSet<string>{"the"}, DisallowBoundaryCross:true);

        var anchors = AnchorDiscovery.SelectAnchors(book, sent, asr, policy, bookStart:10, bookEnd:16);
        Assert.Contains(anchors, a => a.Bp == 12 && a.Ap == 1);
        Assert.DoesNotContain(anchors, a => a.Bp < 10 || a.Bp > 16);
    }

    [Fact]
    public void Preprocessor_And_Pipeline_Map_And_Restrict()
    {
        var bookTokens = new List<string> { "a","b","c","d","e","f","g","h","i","j","k","l","black","forest","m","n","o" };
        var words = new List<BookWord>();
        for (int i = 0; i < bookTokens.Count; i++)
        {
            words.Add(new BookWord(bookTokens[i], i, 0, 0, 0));
        }

        var sentences = new[]
        {
            new BookSentence(0, 0, words.Count - 1)
        };

        var paragraphs = new[]
        {
            new BookParagraph(0, 0, words.Count - 1, "Body", "Normal")
        };

        var sections = new[] { new SectionRange(0, "Chapter 1", 1, "Heading", 10, 16, 0, 0) };

        var totals = new BookTotals(
            Words: words.Count,
            Sentences: 1,
            Paragraphs: 1,
            EstimatedDurationSec: 1
        );

        var book = new BookIndex(
            SourceFile: "fake.docx",
            SourceFileHash: "hash",
            IndexedAt: DateTime.UtcNow,
            Title: null,
            Author: null,
            Totals: totals,
            Words: words.ToArray(),
            Sentences: sentences,
            Paragraphs: paragraphs,
            Sections: sections,
            BuildWarnings: Array.Empty<string>()
        );

        var asr = new AsrResponse("model", new[]
        {
            new AsrToken(0, 1, "x"),
            new AsrToken(1, 1, "black"),
            new AsrToken(2, 1, "forest")
        });

        var policy = new AnchorPolicy(NGram:2, TargetPerTokens:50,
            Stopwords: new HashSet<string>{"the"}, DisallowBoundaryCross:true);

        var secOpts = new SectionDetectOptions(Detect:false);
        var res = AnchorPipeline.ComputeAnchors(book, asr, policy, secOpts, includeWindows:true);

        Assert.Contains(res.Anchors, a => a.Bp >= res.BookWindowFiltered.bStart && a.Bp <= res.BookWindowFiltered.bEnd);
        Assert.True(res.BookFilteredCount > 0 && res.AsrFilteredCount > 0);
        Assert.NotNull(res.Windows);
    }
}
