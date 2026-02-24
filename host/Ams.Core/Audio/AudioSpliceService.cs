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
    /// Generates a roomtone fill buffer of the specified target duration by looping
    /// the provided roomtone sample. If the roomtone is already long enough, it is
    /// trimmed to the exact target length.
    /// </summary>
    /// <param name="roomtone">The source roomtone audio buffer to loop.</param>
    /// <param name="targetDurationSec">The desired fill duration in seconds.</param>
    /// <returns>A new <see cref="AudioBuffer"/> of exactly <paramref name="targetDurationSec"/> length.</returns>
    public static AudioBuffer GenerateRoomtoneFill(AudioBuffer roomtone, double targetDurationSec)
    {
        ArgumentNullException.ThrowIfNull(roomtone);

        if (targetDurationSec <= 0)
            return new AudioBuffer(roomtone.Channels, roomtone.SampleRate, 0, roomtone.Metadata);

        int targetSamples = (int)(targetDurationSec * roomtone.SampleRate);

        // If roomtone is already long enough, just trim to exact length
        if (roomtone.Length >= targetSamples)
            return AudioProcessor.Trim(roomtone, TimeSpan.Zero, TimeSpan.FromSeconds(targetDurationSec));

        // Loop: copy roomtone repeatedly until we reach target length
        var result = new AudioBuffer(roomtone.Channels, roomtone.SampleRate, targetSamples, roomtone.Metadata);
        for (int ch = 0; ch < roomtone.Channels; ch++)
        {
            var src = roomtone.Planar[ch];
            var dst = result.Planar[ch];
            int cursor = 0;
            while (cursor < targetSamples)
            {
                int toCopy = Math.Min(roomtone.Length, targetSamples - cursor);
                Array.Copy(src, 0, dst, cursor, toCopy);
                cursor += toCopy;
            }
        }

        return result;
    }

    /// <summary>
    /// Removes the audio between <paramref name="startSec"/> and <paramref name="endSec"/>
    /// and joins the remaining before/after segments with a crossfade transition.
    /// This avoids the zero-length replacement buffer issue by directly joining the two halves.
    /// </summary>
    /// <param name="original">The full original audio buffer.</param>
    /// <param name="startSec">Start of the region to delete (seconds).</param>
    /// <param name="endSec">End of the region to delete (seconds).</param>
    /// <param name="crossfadeSec">Crossfade duration in seconds (default 30ms).</param>
    /// <param name="curve">Crossfade curve type (default "tri" for triangular).</param>
    /// <returns>A new <see cref="AudioBuffer"/> with the region removed.</returns>
    public static AudioBuffer DeleteRegion(
        AudioBuffer original,
        double startSec,
        double endSec,
        double crossfadeSec = 0.030,
        string curve = "tri")
    {
        ArgumentNullException.ThrowIfNull(original);

        if (startSec < 0)
            throw new ArgumentOutOfRangeException(nameof(startSec), "Start time must be non-negative.");
        if (endSec <= startSec)
            throw new ArgumentOutOfRangeException(nameof(endSec), "End time must be greater than start time.");
        if (string.IsNullOrWhiteSpace(curve))
            curve = "tri";

        var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(startSec));
        var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(endSec));

        // If either segment is empty/negligible, return the other
        if (before.Length == 0 && after.Length == 0)
            return new AudioBuffer(original.Channels, original.SampleRate, 0, original.Metadata);
        if (before.Length == 0)
            return after;
        if (after.Length == 0)
            return before;

        // Crossfade the two halves together
        double clampedCrossfade = ClampCrossfade(
            crossfadeSec,
            DurationSeconds(before),
            DurationSeconds(after));

        return Crossfade(before, after, clampedCrossfade, curve);
    }

    /// <summary>
    /// Inserts audio at a single time point in the original buffer, applying crossfade
    /// transitions at both splice points. This avoids the start==end validation issue
    /// in <see cref="ReplaceSegment"/> by splitting the original at a single point.
    /// </summary>
    /// <param name="original">The full original audio buffer.</param>
    /// <param name="insertPointSec">The time point (seconds) at which to insert audio.</param>
    /// <param name="insertion">The audio buffer to insert.</param>
    /// <param name="crossfadeSec">Crossfade duration in seconds (default 30ms).</param>
    /// <param name="curve">Crossfade curve type (default "tri" for triangular).</param>
    /// <returns>A new <see cref="AudioBuffer"/> with the insertion spliced in.</returns>
    public static AudioBuffer InsertAtPoint(
        AudioBuffer original,
        double insertPointSec,
        AudioBuffer insertion,
        double crossfadeSec = 0.030,
        string curve = "tri")
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(insertion);

        if (insertPointSec < 0)
            throw new ArgumentOutOfRangeException(nameof(insertPointSec), "Insert point must be non-negative.");
        if (string.IsNullOrWhiteSpace(curve))
            curve = "tri";

        // Resample insertion if sample rates differ
        var resampled = insertion.SampleRate != original.SampleRate
            ? AudioProcessor.Resample(insertion, (ulong)original.SampleRate)
            : insertion;

        var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(insertPointSec));
        var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(insertPointSec));

        // Crossfade before + insertion
        double clampedCrossfade1 = ClampCrossfade(
            crossfadeSec,
            DurationSeconds(before),
            DurationSeconds(resampled));

        var joined = Crossfade(before, resampled, clampedCrossfade1, curve);

        // Crossfade joined + after
        double clampedCrossfade2 = ClampCrossfade(
            crossfadeSec,
            DurationSeconds(joined),
            DurationSeconds(after));

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
