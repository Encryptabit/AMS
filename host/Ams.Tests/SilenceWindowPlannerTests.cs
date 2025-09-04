using System;
using System.Collections.Generic;
using Ams.Core;
using Xunit;

public class SilenceWindowPlannerTests
{
    [Fact]
    public void Plans_DenseCandidates_Strict()
    {
        var p = new SilenceWindowPlanner();
        double D = 300.0; // 5 min
        // Candidate mids roughly every ~75s, with some jitter
        var cuts = new List<double> { 74.8, 150.5, 226.2 };
        var seg = new SegmentationParams(60, 90, 75, true);
        var plan = p.Plan(D, cuts, seg);
        Assert.True(plan.Spans.Count >= 3);
        foreach (var s in plan.Spans)
        {
            Assert.InRange(s.Length, seg.Min - 1e-6, seg.Max + 1e-6);
        }
    }

    [Fact]
    public void Fails_When_NoValidPath_Strict()
    {
        var p = new SilenceWindowPlanner();
        double D = 200.0; // not divisible nicely
        var cuts = new List<double>(); // no candidates: only 0 and D
        var seg = new SegmentationParams(60, 90, 75, true);
        Assert.Throws<InvalidOperationException>(() => p.Plan(D, cuts, seg));
    }

    [Fact]
    public void RelaxesTail_When_Allowed()
    {
        var p = new SilenceWindowPlanner();
        double D = 200.0;
        var cuts = new List<double> { 75.0, 150.0 };
        var seg = new SegmentationParams(60, 90, 75, false);
        var plan = p.Plan(D, cuts, seg);
        Assert.True(plan.TailRelaxed);
        Assert.True(plan.Spans.Count >= 2);
    }
}

