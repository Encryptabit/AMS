namespace Ams.Core.Audio;

/// <summary>
/// Options for audio treatment processing.
/// </summary>
public sealed record TreatmentOptions
{
    /// <summary>
    /// Duration of roomtone before chapter title (seconds).
    /// </summary>
    public double PrerollSeconds { get; init; } = 0.75;

    /// <summary>
    /// Duration of roomtone between chapter title and content (seconds).
    /// </summary>
    public double ChapterToContentGapSeconds { get; init; } = 1.5;

    /// <summary>
    /// Duration of roomtone after chapter content (seconds).
    /// </summary>
    public double PostrollSeconds { get; init; } = 3.0;

    /// <summary>
    /// Threshold in dB for silence detection (used to find speech boundaries).
    /// </summary>
    public double SilenceThresholdDb { get; init; } = -55.0;

    /// <summary>
    /// Minimum duration of silence to consider as a gap (seconds).
    /// </summary>
    public double MinimumSilenceDuration { get; init; } = 0.5;

    /// <summary>
    /// Minimum gap duration to consider as title/content boundary (seconds).
    /// </summary>
    public double TitleContentGapThreshold { get; init; } = 1.0;

    /// <summary>
    /// Crossfade duration in seconds used when splicing treatment segments.
    /// </summary>
    public double SpliceCrossfadeDurationSec { get; init; } = 0.070;

    /// <summary>
    /// FFmpeg acrossfade curve used at treatment splice joins.
    /// </summary>
    public string SpliceCrossfadeCurve { get; init; } = "hsin";
}
