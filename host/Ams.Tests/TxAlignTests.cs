using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Align.Tx;
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

