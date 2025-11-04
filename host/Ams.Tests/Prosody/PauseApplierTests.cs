using System.Collections.Generic;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Prosody;
using Xunit;
using SentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Tests.Prosody;

public sealed class PauseApplierTests
{
    [Fact]
    public void TimelineApplier_ShiftsSubsequentSentences()
    {
        var baseline = new Dictionary<int, SentenceTiming>
        {
            [1] = new SentenceTiming(0.0, 1.0),
            [2] = new SentenceTiming(1.5, 2.5),
            [3] = new SentenceTiming(3.0, 4.0)
        };

        var adjustments = new List<PauseAdjust>
        {
            new PauseAdjust(1, 2, PauseClass.Sentence, 0.5, 0.2, 1.0, 1.5, false)
        };

        var result = PauseTimelineApplier.Apply(baseline, adjustments).Timeline;

        Assert.Equal(0.0, result[1].StartSec, 6);
        Assert.Equal(1.0, result[1].EndSec, 6);

        Assert.Equal(1.2, result[2].StartSec, 6);
        Assert.Equal(2.2, result[2].EndSec, 6);

        Assert.Equal(2.7, result[3].StartSec, 6);
        Assert.Equal(3.7, result[3].EndSec, 6);
    }

    // [Fact]
    // public void AudioApplier_AdjustsGapLength()
    // {
    //     const int sampleRate = 1000;
    //     var audio = new AudioBuffer(1, sampleRate, 1000);
    //     for (int i = 0; i < audio.Length; i++)
    //     {
    //         audio.Planar[0][i] = i;
    //     }
    //
    //     var roomtone = new AudioBuffer(1, sampleRate, 100);
    //     for (int i = 0; i < roomtone.Length; i++)
    //     {
    //         roomtone.Planar[0][i] = 0f;
    //     }
    //
    //     var sentences = new List<SentenceAlign>
    //     {
    //         new SentenceAlign(
    //             Id: 1,
    //             BookRange: new IntRange(0, 0),
    //             ScriptRange: null,
    //             Timing: new TimingRange(0.0, 0.5),
    //             Metrics: new SentenceMetrics(0, 0, 0, 0, 0),
    //             Status: "ok"),
    //         new SentenceAlign(
    //             Id: 2,
    //             BookRange: new IntRange(0, 0),
            //             ScriptRange: null,
    //             Timing: new TimingRange(0.4, 0.9),
    //             Metrics: new SentenceMetrics(0, 0, 0, 0, 0),
    //             Status: "ok")
    //     };
    //
    //     var updatedTimeline = new Dictionary<int, SentenceTiming>
    //     {
    //         [1] = new SentenceTiming(0.0, 0.5),
    //         [2] = new SentenceTiming(0.6, 1.1)
    //     };
    //
    //     var adjusted = PauseAudioApplier.Apply(
    //         audio,
    //         roomtone,
    //         sentences,
    //         updatedTimeline,
    //         toneGainLinear: 1.0,
    //         fadeMs: 10.0);
    //
    //     Assert.Equal(1100, adjusted.Length);
    //
    //     AssertWithinTolerance(audio.Planar[0], 0, adjusted.Planar[0], 0, 500);
    //     AssertWithinTolerance(audio.Planar[0], 400, adjusted.Planar[0], 600, 500);
    //
    //     int gapSamples = (int)Math.Round((updatedTimeline[2].StartSec - updatedTimeline[1].EndSec) * sampleRate);
    //     Assert.Equal(100, gapSamples);
    // }
}
