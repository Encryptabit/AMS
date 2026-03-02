using Ams.Core.Audio.QualityControl;

namespace Ams.Tests.Audio.QualityControl;

public class AudioQcAnalyzerTests
{
    #region AnalyzeStructure

    [Fact]
    public void AnalyzeStructure_TypicalChapter_IdentifiesAllStructure()
    {
        // Head silence 0-0.75, title speech 0.75-3.2, title-body gap 3.2-4.8, body, tail silence 297.5-300.1
        var silences = new SilenceRegion[]
        {
            new(0.0, 0.75, 0.75),
            new(3.2, 4.8, 1.6),
            new(297.5, 300.1, 2.6)
        };

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.1);

        Assert.Equal(0.75, head);
        Assert.NotNull(title);
        Assert.Equal(3.2 - 0.75, title.Value, 3); // title duration = speech between head end and gap start
        Assert.Equal(1.6, gap);
        Assert.Equal(2.6, tail);
    }

    [Fact]
    public void AnalyzeStructure_NoSilences_ReturnsZeros()
    {
        var silences = Array.Empty<SilenceRegion>();

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.0);

        Assert.Equal(0.0, head);
        Assert.Null(title);
        Assert.Null(gap);
        Assert.Equal(0.0, tail);
    }

    [Fact]
    public void AnalyzeStructure_OnlyHeadSilence_NoTitleOrGap()
    {
        var silences = new SilenceRegion[]
        {
            new(0.0, 0.8, 0.8)
        };

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.0);

        Assert.Equal(0.8, head);
        Assert.Null(title);
        Assert.Null(gap);
        Assert.Equal(0.0, tail);
    }

    [Fact]
    public void AnalyzeStructure_HeadAndTailOnly_NoTitleOrGap()
    {
        var silences = new SilenceRegion[]
        {
            new(0.0, 0.8, 0.8),
            new(297.0, 300.0, 3.0)
        };

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.0);

        Assert.Equal(0.8, head);
        Assert.Null(title);
        Assert.Null(gap);
        Assert.Equal(3.0, tail);
    }

    [Fact]
    public void AnalyzeStructure_OpenEndedTrailingSilence_UsesTotalDuration()
    {
        // Open-ended silence (End = -1) should use totalDuration to compute duration
        var silences = new SilenceRegion[]
        {
            new(0.0, 0.75, 0.75),
            new(295.3, -1.0, -1.0)
        };

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.0);

        Assert.Equal(0.75, head);
        Assert.Equal(300.0 - 295.3, tail, 3); // 4.7s
    }

    [Fact]
    public void AnalyzeStructure_HeadNotNearZero_NoHeadSilence()
    {
        // First silence doesn't start near 0 -- not head silence
        var silences = new SilenceRegion[]
        {
            new(5.0, 6.0, 1.0),
            new(297.0, 300.0, 3.0)
        };

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.0);

        Assert.Equal(0.0, head);
        Assert.Equal(3.0, tail);
    }

    [Fact]
    public void AnalyzeStructure_SingleSilenceNearEnd_TailOnly()
    {
        var silences = new SilenceRegion[]
        {
            new(298.0, 300.0, 2.0)
        };

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.0);

        Assert.Equal(0.0, head);
        Assert.Null(title);
        Assert.Null(gap);
        Assert.Equal(2.0, tail);
    }

    [Fact]
    public void AnalyzeStructure_MicroPausesBeforeGap_SelectsLongestAsGap()
    {
        // Head silence, then several micro-pauses (inter-word), then the real title-body gap
        var silences = new SilenceRegion[]
        {
            new(0.0, 0.75, 0.75),       // head
            new(0.88, 0.95, 0.07),       // micro-pause within title
            new(1.10, 1.17, 0.07),       // micro-pause within title
            new(3.20, 4.80, 1.60),       // actual title-body gap (longest)
            new(8.50, 8.58, 0.08),       // micro-pause in body
            new(297.5, 300.1, 2.60)      // tail
        };

        var (head, title, gap, tail) = AudioQcAnalyzer.AnalyzeStructure(silences, 300.1);

        Assert.Equal(0.75, head);
        Assert.NotNull(gap);
        Assert.Equal(1.6, gap.Value, 3);
        Assert.NotNull(title);
        Assert.Equal(3.20 - 0.75, title.Value, 3); // title = speech between head end and gap start
        Assert.Equal(2.6, tail);
    }

    #endregion

    #region FlagAnomalies

    [Fact]
    public void FlagAnomalies_AllWithinThresholds_NoFlags()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.75,
            TitleBodyGapSec = 1.5,
            TailSilenceSec = 3.0
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Empty(flags);
    }

    [Fact]
    public void FlagAnomalies_HeadSilenceTooShort_Flagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.3,
            TailSilenceSec = 3.0
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Single(flags);
        Assert.Contains("HEAD_SILENCE_SHORT", flags[0]);
        Assert.Contains("0.30s", flags[0]);
        Assert.Contains("0.50s", flags[0]);
    }

    [Fact]
    public void FlagAnomalies_HeadSilenceTooLong_Flagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 1.5,
            TailSilenceSec = 3.0
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Single(flags);
        Assert.Contains("HEAD_SILENCE_LONG", flags[0]);
    }

    [Fact]
    public void FlagAnomalies_TailSilenceTooShort_Flagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.75,
            TailSilenceSec = 1.0
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Single(flags);
        Assert.Contains("TAIL_SILENCE_SHORT", flags[0]);
    }

    [Fact]
    public void FlagAnomalies_TailSilenceTooLong_Flagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.75,
            TailSilenceSec = 6.1
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Single(flags);
        Assert.Contains("TAIL_SILENCE_LONG", flags[0]);
    }

    [Fact]
    public void FlagAnomalies_TitleBodyGapTooShort_Flagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.75,
            TitleBodyGapSec = 0.5,
            TailSilenceSec = 3.0
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Single(flags);
        Assert.Contains("TITLE_GAP_SHORT", flags[0]);
    }

    [Fact]
    public void FlagAnomalies_TitleBodyGapTooLong_Flagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.75,
            TitleBodyGapSec = 3.0,
            TailSilenceSec = 3.0
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Single(flags);
        Assert.Contains("TITLE_GAP_LONG", flags[0]);
    }

    [Fact]
    public void FlagAnomalies_NullTitleBodyGap_NotFlagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.75,
            TitleBodyGapSec = null,
            TailSilenceSec = 3.0
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Empty(flags);
    }

    [Fact]
    public void FlagAnomalies_MultipleViolations_AllFlagged()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.2,
            TitleBodyGapSec = 3.5,
            TailSilenceSec = 6.5
        };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, new QcThresholds());

        Assert.Equal(3, flags.Count);
        Assert.Contains(flags, f => f.Contains("HEAD_SILENCE_SHORT"));
        Assert.Contains(flags, f => f.Contains("TITLE_GAP_LONG"));
        Assert.Contains(flags, f => f.Contains("TAIL_SILENCE_LONG"));
    }

    [Fact]
    public void FlagAnomalies_CustomThresholds_Respected()
    {
        var result = new ChapterQcResult
        {
            FileName = "test.mp3",
            DurationSec = 300.0,
            HeadSilenceSec = 0.75,
            TailSilenceSec = 3.0
        };

        // Custom thresholds that make 0.75 head silence too long
        var thresholds = new QcThresholds { MaxHeadSilence = 0.5 };

        var flags = AudioQcAnalyzer.FlagAnomalies(result, thresholds);

        Assert.Single(flags);
        Assert.Contains("HEAD_SILENCE_LONG", flags[0]);
    }

    #endregion
}
