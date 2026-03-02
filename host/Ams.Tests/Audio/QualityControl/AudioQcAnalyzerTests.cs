using Ams.Core.Audio.QualityControl;

namespace Ams.Tests.Audio.QualityControl;

public class AudioQcAnalyzerTests
{
    #region ParseSilenceRegions

    [Fact]
    public void ParseSilenceRegions_StandardOutput_ReturnsSilenceRegions()
    {
        var stderr = """
            [silencedetect @ 0x1234] silence_start: 0
            [silencedetect @ 0x1234] silence_end: 0.752 | silence_duration: 0.752
            [silencedetect @ 0x1234] silence_start: 3.210
            [silencedetect @ 0x1234] silence_end: 4.860 | silence_duration: 1.650
            [silencedetect @ 0x1234] silence_start: 297.500
            [silencedetect @ 0x1234] silence_end: 300.100 | silence_duration: 2.600
            """;

        var regions = AudioQcAnalyzer.ParseSilenceRegions(stderr);

        Assert.Equal(3, regions.Length);
        Assert.Equal(0.0, regions[0].Start);
        Assert.Equal(0.752, regions[0].End);
        Assert.Equal(0.752, regions[0].Duration);
        Assert.Equal(3.210, regions[1].Start);
        Assert.Equal(4.860, regions[1].End);
        Assert.Equal(1.650, regions[1].Duration);
        Assert.Equal(297.500, regions[2].Start);
        Assert.Equal(300.100, regions[2].End);
        Assert.Equal(2.600, regions[2].Duration);
    }

    [Fact]
    public void ParseSilenceRegions_EmptyOutput_ReturnsEmptyList()
    {
        var regions = AudioQcAnalyzer.ParseSilenceRegions("");
        Assert.Empty(regions);
    }

    [Fact]
    public void ParseSilenceRegions_NoSilenceDetected_ReturnsEmptyList()
    {
        var stderr = """
            Input #0, wav, from 'test.wav':
              Duration: 00:05:00.00, bitrate: 768 kb/s
            Stream #0:0: Audio: pcm_s16le, 48000 Hz, mono, s16, 768 kb/s
            size= 0kB time=00:05:00.00 bitrate=N/A speed=250x
            """;

        var regions = AudioQcAnalyzer.ParseSilenceRegions(stderr);
        Assert.Empty(regions);
    }

    [Fact]
    public void ParseSilenceRegions_TrailingSilenceStart_WithNoEnd_ReturnsOpenEndedRegion()
    {
        // When silence_start has no matching silence_end, the silence extends to end of file.
        // We track it with End = -1 and Duration = -1 to signal open-ended.
        var stderr = """
            [silencedetect @ 0x1234] silence_start: 0
            [silencedetect @ 0x1234] silence_end: 0.8 | silence_duration: 0.8
            [silencedetect @ 0x1234] silence_start: 295.3
            """;

        var regions = AudioQcAnalyzer.ParseSilenceRegions(stderr);

        Assert.Equal(2, regions.Length);
        Assert.Equal(0.0, regions[0].Start);
        Assert.Equal(0.8, regions[0].End);
        Assert.Equal(295.3, regions[1].Start);
        // Open-ended: End and Duration are -1 (sentinel)
        Assert.Equal(-1.0, regions[1].End);
        Assert.Equal(-1.0, regions[1].Duration);
    }

    [Fact]
    public void ParseSilenceRegions_HandlesInvariantCulture()
    {
        // Ensure that decimal parsing works regardless of locale
        var stderr = """
            [silencedetect @ 0x1234] silence_start: 0.000
            [silencedetect @ 0x1234] silence_end: 0.750 | silence_duration: 0.750
            """;

        var regions = AudioQcAnalyzer.ParseSilenceRegions(stderr);

        Assert.Single(regions);
        Assert.Equal(0.75, regions[0].End);
    }

    [Fact]
    public void ParseSilenceRegions_MixedOutputWithOtherLines_IgnoresNonSilenceLines()
    {
        var stderr = """
            ffmpeg version 6.1 Copyright (c) 2000-2023
            Input #0, mp3, from 'test.mp3':
              Duration: 00:03:25.00, start: 0.025057, bitrate: 320 kb/s
            [silencedetect @ 0x1234] silence_start: 0
            Stream mapping:
              Stream #0:0 -> #0:0 (mp3 (native) -> pcm_s16le (native))
            [silencedetect @ 0x1234] silence_end: 0.8 | silence_duration: 0.8
            size=N/A time=00:03:25.00 bitrate=N/A speed=150x
            """;

        var regions = AudioQcAnalyzer.ParseSilenceRegions(stderr);

        Assert.Single(regions);
        Assert.Equal(0.0, regions[0].Start);
        Assert.Equal(0.8, regions[0].End);
    }

    #endregion

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
