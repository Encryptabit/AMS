using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Core.Audio;

/// <summary>
/// Provides stateless audio splicing operations for replacing time ranges
/// in an <see cref="AudioBuffer"/> with a replacement buffer, applying
/// crossfade transitions at both splice points via FFmpeg's acrossfade filter.
/// </summary>
public static class AudioSpliceService
{
    /// <summary>
    /// Replaces the audio between <paramref name="startSec"/> and <paramref name="endSec"/>
    /// in <paramref name="original"/> with <paramref name="replacement"/>, applying crossfade
    /// transitions at both splice points.
    /// </summary>
    /// <param name="original">The full original audio buffer.</param>
    /// <param name="startSec">Start of the segment to replace (seconds).</param>
    /// <param name="endSec">End of the segment to replace (seconds).</param>
    /// <param name="replacement">The replacement audio buffer to splice in.</param>
    /// <param name="crossfadeSec">Crossfade duration in seconds (default 30ms). Clamped to avoid exceeding segment boundaries.</param>
    /// <param name="curve">Crossfade curve type (default "tri" for triangular).</param>
    /// <returns>A new <see cref="AudioBuffer"/> with the replacement spliced in.</returns>
    public static AudioBuffer ReplaceSegment(
        AudioBuffer original,
        double startSec,
        double endSec,
        AudioBuffer replacement,
        double crossfadeSec = 0.030,
        string curve = "tri")
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(replacement);

        if (startSec < 0)
            throw new ArgumentOutOfRangeException(nameof(startSec), "Start time must be non-negative.");
        if (endSec <= startSec)
            throw new ArgumentOutOfRangeException(nameof(endSec), "End time must be greater than start time.");
        if (string.IsNullOrWhiteSpace(curve))
            curve = "tri";

        // Step 1: Resample replacement if sample rates differ
        var resampled = replacement.SampleRate != original.SampleRate
            ? AudioProcessor.Resample(replacement, (ulong)original.SampleRate)
            : replacement;

        // Step 2: Trim before and after segments from original
        var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(startSec));
        var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(endSec));

        // Step 3: Clamp crossfade for first join (before + replacement)
        double clampedCrossfade1 = ClampCrossfade(
            crossfadeSec,
            DurationSeconds(before),
            DurationSeconds(resampled));

        // Step 4: Crossfade before + replacement
        var joined = Crossfade(before, resampled, clampedCrossfade1, curve);

        // Step 5: Clamp crossfade for second join (joined + after)
        double clampedCrossfade2 = ClampCrossfade(
            crossfadeSec,
            DurationSeconds(joined),
            DurationSeconds(after));

        // Step 6: Crossfade joined + after
        return Crossfade(joined, after, clampedCrossfade2, curve);
    }

    /// <summary>
    /// Computes the duration in seconds of an <see cref="AudioBuffer"/>.
    /// </summary>
    private static double DurationSeconds(AudioBuffer buffer)
        => (double)buffer.Length / buffer.SampleRate;

    /// <summary>
    /// Clamps the crossfade duration to 30% of the shorter of the two segments,
    /// preventing the crossfade from exceeding segment boundaries.
    /// </summary>
    private static double ClampCrossfade(double requestedSec, double aDurationSec, double bDurationSec)
        => Math.Min(requestedSec, Math.Min(aDurationSec * 0.3, bDurationSec * 0.3));

    /// <summary>
    /// Crossfades two audio buffers using FFmpeg's acrossfade filter.
    /// Falls back to simple concatenation when crossfade is negligible or a buffer is empty.
    /// </summary>
    private static AudioBuffer Crossfade(AudioBuffer a, AudioBuffer b, double durationSec, string curve)
    {
        // Fall back to concat for negligible crossfade or empty buffers
        if (durationSec <= 0.001 || a.Length == 0 || b.Length == 0)
            return AudioBuffer.Concat(a, b);

        var inputs = new[]
        {
            new FfFilterGraphRunner.GraphInput("a", a),
            new FfFilterGraphRunner.GraphInput("b", b)
        };

        var filterSpec = FormattableString.Invariant(
            $"[a]aformat=sample_fmts=flt[af];[b]aformat=sample_fmts=flt[bf];[af][bf]acrossfade=d={durationSec:F6}:c1={curve}:c2={curve}[out]");

        return FfFilterGraphRunner.Apply(inputs, filterSpec);
    }
}
