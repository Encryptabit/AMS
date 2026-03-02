namespace Ams.Core.Audio;

/// <summary>
/// Shared silence detection constants used across the application.
/// These values were determined empirically from audiobook narration recordings
/// and represent good defaults for ASR pre-chunking and splice boundary detection.
/// </summary>
public static class AudioDefaults
{
    /// <summary>
    /// Default silence threshold in decibels. Signals below this level are
    /// considered silence for boundary detection purposes.
    /// </summary>
    public const double SilenceThresholdDb = -55.0;

    /// <summary>
    /// Minimum silence duration for chunk boundary detection.
    /// The 200ms value prevents boundaries from landing on micro-pauses that are
    /// too brief for clean ASR segmentation. Prior 50ms matched FFmpeg silence-detect
    /// sensitivity but was too aggressive for pre-chunking use cases.
    /// </summary>
    public static readonly TimeSpan MinimumSilenceDuration = TimeSpan.FromMilliseconds(200);
}
