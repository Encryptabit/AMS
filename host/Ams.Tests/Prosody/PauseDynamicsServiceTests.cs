using System;
using System.Collections.Generic;
using Ams.Core.Prosody;
using Xunit;

namespace Ams.Tests.Prosody;

public sealed class PauseDynamicsServiceTests
{
    private static PauseSpan CreateSpan(
        int leftSentenceId,
        int rightSentenceId,
        double startSec,
        double endSec,
        PauseClass pauseClass,
        bool hasGapHint = false)
    {
        var duration = Math.Max(0d, endSec - startSec);
        return new PauseSpan(
            leftSentenceId,
            rightSentenceId,
            startSec,
            endSec,
            duration,
            pauseClass,
            hasGapHint,
            CrossesParagraph: false,
            CrossesChapterHead: false,
            Provenance: PauseProvenance.ScriptPunctuation);
    }

    [Fact]
    public void PlanTransforms_CompressesSentencePauseOutsideWindow()
    {
        var span = CreateSpan(1, 2, 0d, 2d, PauseClass.Sentence);
        var spans = new List<PauseSpan> { span };
        var classes = new Dictionary<PauseClass, PauseClassSummary>
        {
            [PauseClass.Sentence] = PauseClassSummary.FromDurations(new[] { span.DurationSec })
        };

        var analysis = new PauseAnalysisReport(spans, classes);
        var policy = PausePolicyPresets.House();
        var service = new PauseDynamicsService();

        var transforms = service.PlanTransforms(analysis, policy);

        var adjust = Assert.Single(transforms.PauseAdjusts);
        Assert.Equal(PauseClass.Sentence, adjust.Class);
        Assert.Equal(span.DurationSec, adjust.OriginalDurationSec);
        Assert.True(adjust.TargetDurationSec < span.DurationSec);
        Assert.InRange(adjust.TargetDurationSec, policy.Sentence.Min, policy.Sentence.Max + policy.KneeWidth);
        Assert.Equal(span.StartSec, adjust.StartSec);
        Assert.Equal(span.EndSec, adjust.EndSec);
    }

    [Fact]
    public void PlanTransforms_PreservesTopQuantileForLongestGap()
    {
        var spanA = CreateSpan(1, 2, 0d, 1.3d, PauseClass.Sentence);
        var spanB = CreateSpan(2, 3, 2d, 3.8d, PauseClass.Sentence);
        var spans = new List<PauseSpan> { spanA, spanB };
        var classes = new Dictionary<PauseClass, PauseClassSummary>
        {
            [PauseClass.Sentence] = PauseClassSummary.FromDurations(new[]
            {
                spanA.DurationSec,
                spanB.DurationSec
            })
        };

        var policy = new PausePolicy(
            new PauseWindow(0.20, 0.50),
            new PauseWindow(0.60, 1.00),
            new PauseWindow(1.10, 1.40),
            headOfChapter: 0.75,
            postChapterRead: 1.50,
            tail: 3.00,
            kneeWidth: 0.08,
            ratioInside: 1.25,
            ratioOutside: 3.0,
            preserveTopQuantile: 1.0);

        var service = new PauseDynamicsService();
        var analysis = new PauseAnalysisReport(spans, classes);

        var transforms = service.PlanTransforms(analysis, policy);

        Assert.Contains(transforms.PauseAdjusts, adjust => Math.Abs(adjust.OriginalDurationSec - spanA.DurationSec) < 1e-6);
        Assert.DoesNotContain(transforms.PauseAdjusts, adjust => Math.Abs(adjust.OriginalDurationSec - spanB.DurationSec) < 1e-6);
    }
}
