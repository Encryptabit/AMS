namespace Ams.Core.Audio.QualityControl;

/// <summary>
/// Represents a single silence region detected by ffmpeg silencedetect.
/// </summary>
public sealed record SilenceRegion(double Start, double End, double Duration);

/// <summary>
/// Thresholds for flagging QC anomalies (all values in seconds).
/// </summary>
public sealed record QcThresholds
{
    public double MinHeadSilence { get; init; } = 0.5;
    public double MaxHeadSilence { get; init; } = 1.0;
    public double MinTailSilence { get; init; } = 2.0;
    public double MaxTailSilence { get; init; } = 5.0;
    public double MinTitleBodyGap { get; init; } = 1.0;
    public double MaxTitleBodyGap { get; init; } = 2.5;
}

/// <summary>
/// Per-file QC analysis result describing the chapter audio structure.
/// </summary>
public sealed record ChapterQcResult
{
    public required string FileName { get; init; }
    public required double DurationSec { get; init; }
    public int Channels { get; init; }
    public int SampleRate { get; init; }
    public double HeadSilenceSec { get; init; }
    public double? TitleDurationSec { get; init; }
    public double? TitleBodyGapSec { get; init; }
    public double TailSilenceSec { get; init; }
    public IReadOnlyList<SilenceRegion> AllSilences { get; init; } = [];
    public IReadOnlyList<string> Flags { get; init; } = [];
}
