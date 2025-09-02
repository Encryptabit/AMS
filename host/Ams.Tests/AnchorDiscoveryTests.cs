using System.Collections.Generic;
using Ams.Align.Anchors;
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
