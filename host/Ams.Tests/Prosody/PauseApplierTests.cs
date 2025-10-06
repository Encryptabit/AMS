using System.Collections.Generic;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Prosody;
using Xunit;

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

        var result = PauseTimelineApplier.Apply(baseline, adjustments);

        Assert.Equal(0.0, result[1].StartSec, 6);
        Assert.Equal(1.0, result[1].EndSec, 6);

        Assert.Equal(1.2, result[2].StartSec, 6);
        Assert.Equal(2.2, result[2].EndSec, 6);

        Assert.Equal(2.7, result[3].StartSec, 6);
        Assert.Equal(3.7, result[3].EndSec, 6);
    }

    [Fact]
    public void AudioApplier_AdjustsGapLength()
    {
        const int sampleRate = 1000;
        var audio = new AudioBuffer(1, sampleRate, 1000);
        for (int i = 0; i < audio.Length; i++)
        {
            audio.Planar[0][i] = i;
        }

        var roomtone = new AudioBuffer(1, sampleRate, 100);
        for (int i = 0; i < roomtone.Length; i++)
        {
            roomtone.Planar[0][i] = 0f;
        }

        var adjustments = new List<PauseAdjust>
        {
            new PauseAdjust(1, 2, PauseClass.Sentence, 0.3, 0.1, 0.1, 0.4, false)
        };

        var adjusted = PauseAudioApplier.Apply(audio, roomtone, adjustments, toneGainLinear: 1.0, overlapMs: 10);

        Assert.Equal(800, adjusted.Length);
        Assert.Equal(audio.Planar[0][50], adjusted.Planar[0][50]);
        Assert.Equal(audio.Planar[0][450], adjusted.Planar[0][250]);
    }
}
