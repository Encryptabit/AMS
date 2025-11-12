using System.Collections.Generic;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core;
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
        // With stopword filter and fallback to bigrams, expect anchor at (1,1) for "black|forest"
        Assert.Contains(anchors, a => a.Bp == 1 && a.Ap == 1);
    }

    [Fact]
    public void LisEnforcesMonotonicity()
    {
        var lis = AnchorDiscovery.LisByAp(new List<(int,int)>{(10,50),(20,40),(30,60)});
        Assert.Equal(2, lis.Count);
        Assert.True(lis[0].Item2 < lis[1].Item2); // ap strictly increasing
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
        // Minimal fake index with two sections around word ranges
        var words = new List<BookWord>();
        for (int i = 0; i < 100; i++)
        {
            words.Add(new BookWord($"w{i}", i, i/5, i/10, -1));
        }

        var sections = new[]
        {
            new SectionRange(0, "Prologue", 1, "Heading", 0, 29, 0, 2),
            new SectionRange(1, "Chapter 14: Storm", 1, "Heading", 30, 99, 3, 9)
        };

        return new BookIndex(
            SourceFile: "fake.docx",
            SourceFileHash: "hash",
            IndexedAt: System.DateTime.UtcNow,
            Title: "Test",
            Author: "Author",
            Totals: new BookTotals(100, 20, 10, 30.0),
            Words: words.ToArray(),
            Sentences: new[] { new SentenceRange(0,0,4) },
            Paragraphs: new[] { new ParagraphRange(0,0,9,"","Heading 1") },
            Sections: sections,
            BuildWarnings: null
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

    [Theory]
    [InlineData("Chapter 14 - Storm", 1)]
    [InlineData("chapter fourteen storm", 1)]
    [InlineData("chapter xiv storm", 1)]
    [InlineData("Prologue", 0)]
    public void ResolveSectionByTitle_NormalizesNumbers(string label, int expectedId)
    {
        var book = MakeBookIndex();
        var resolved = SectionLocator.ResolveSectionByTitle(book, label);
        Assert.NotNull(resolved);
        Assert.Equal(expectedId, resolved!.Id);
    }

    [Fact]
    public void AnchorSelection_Respects_Book_Window()
    {
        // Book tokens contain a unique bigram only in the second window
        var book = new List<string> { "a","b","c","d","e","f","g","h","i","j",
                                      "k","l","black","forest","m","n","o" };
        var sent = new List<int>();
        for (int i = 0; i < book.Count; i++) sent.Add(0);
        var asr  = new List<string> { "x","black","forest","y" };

        var policy = new AnchorPolicy(NGram:2, TargetPerTokens:50,
            Stopwords: new HashSet<string>{"the"}, DisallowBoundaryCross:true);

        // Restrict to [10,16] which includes the bigram at indices 12-13
        var anchors = AnchorDiscovery.SelectAnchors(book, sent, asr, policy, bookStart:10, bookEnd:16);
        Assert.Contains(anchors, a => a.Bp == 12 && a.Ap == 1);
        // Ensure we did not produce anchors outside the window
        Assert.DoesNotContain(anchors, a => a.Bp < 10 || a.Bp > 16);
    }

    [Fact]
    public void Preprocessor_And_Pipeline_Map_And_Restrict()
    {
        // Book with unique bigram only in window 12-13
        var bookTokens = new List<string> { "a","b","c","d","e","f","g","h","i","j","k","l","black","forest","m","n","o" };
        var words = new List<BookWord>();
        for (int i = 0; i < bookTokens.Count; i++)
            words.Add(new BookWord(bookTokens[i], i, 0, 0, -1));
        var book = new BookIndex(
            SourceFile: "fake.docx",
            SourceFileHash: "hash",
            IndexedAt: System.DateTime.UtcNow,
            Title: null,
            Author: null,
            Totals: new BookTotals(words.Count, 1, 1, 1),
            Words: words.ToArray(),
            Sentences: new[] { new SentenceRange(0,0,words.Count-1) },
            Paragraphs: new[] { new ParagraphRange(0,0,words.Count-1,"","Body") },
            Sections: new[] { new SectionRange(0, "Chapter 1", 1, "Heading", 10, 16, 0, 0) }
        );

        var asr = new AsrResponse("model", new[] { new AsrToken(0,1,"x"), new AsrToken(1,1,"black"), new AsrToken(2,1,"forest") });

        var policy = new AnchorPolicy(NGram:2, TargetPerTokens:50,
            Stopwords: new HashSet<string>{"the"}, DisallowBoundaryCross:true);

        var secOpts = new SectionDetectOptions(Detect:false);
        var res = AnchorPipeline.ComputeAnchors(book, asr, policy, secOpts, includeWindows:true);

        Assert.Contains(res.Anchors, a => a.Bp >= res.BookWindowFiltered.bStart && a.Bp <= res.BookWindowFiltered.bEnd);
        Assert.True(res.BookFilteredCount > 0 && res.AsrFilteredCount > 0);
        Assert.NotNull(res.Windows);
    }
}

