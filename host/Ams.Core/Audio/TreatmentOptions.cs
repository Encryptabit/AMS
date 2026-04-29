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
    /// Safety padding added around detected speech boundaries before splicing (seconds).
    /// </summary>
    public double BoundaryPaddingSeconds { get; init; } = 0.0;

    /// <summary>
    /// Minimum duration of silence to consider as a gap (seconds).
    /// Matches the QC analyze default so treated output is judged at the same resolution it was produced at.
    /// </summary>
    public double MinimumSilenceDuration { get; init; } = 0.25;

    /// <summary>
    /// Minimum gap duration to consider as title/content boundary (seconds).
    /// </summary>
    public double TitleContentGapThreshold { get; init; } = 1.0;

    /// <summary>
    /// Maximum non-silent burst duration to absorb when coalescing fragmented silence regions.
    /// Brief blips (mic pops, mouth clicks, page rustles, breath pulses) at or below this duration
    /// are treated as silence so that head, tail, and title-body boundary detection sees the
    /// perceptual silence rather than the literal one. Default 150ms covers observed
    /// end-of-chapter noise transients while staying under the duration of any plausible word.
    /// </summary>
    public double ClickImmunityBurstSec { get; init; } = 0.150;

    /// <summary>
    /// Crossfade duration in seconds used when splicing treatment segments.
    /// </summary>
    public double SpliceCrossfadeDurationSec { get; init; } = 0.070;

    /// <summary>
    /// FFmpeg acrossfade curve used at treatment splice joins.
    /// </summary>
    public string SpliceCrossfadeCurve { get; init; } = "hsin";
}
