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
    private static readonly HashSet<string> SupportedCrossfadeCurves = new(StringComparer.Ordinal)
    {
        "nofade",
        "tri",
        "qsin",
        "esin",
        "hsin",
        "log",
        "ipar",
        "qua",
        "cub",
        "squ",
        "cbr",
        "par",
        "exp",
        "iqsin",
        "ihsin",
        "dese",
        "desi",
        "losi",
        "sinc",
        "isinc",
        "quat",
        "quatr",
        "qsin2",
        "hsin2"
    };

    private static readonly Dictionary<string, string> CrossfadeCurveAliases = new(StringComparer.Ordinal)
    {
        ["equal-power"] = "hsin",
        ["equal_power"] = "hsin",
        ["equalpower"] = "hsin",
        ["half-sine"] = "hsin",
        ["half_sine"] = "hsin",
        ["half-sin"] = "hsin",
        ["linear"] = "tri",
        ["triangle"] = "tri",
        ["triangular"] = "tri"
    };

    /// <summary>
    /// Computes the duration in seconds of an <see cref="AudioBuffer"/>.
    /// </summary>
    public static double DurationSeconds(AudioBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return (double)buffer.Length / buffer.SampleRate;
    }

    /// <summary>
    /// Normalizes operator-facing crossfade curve names to FFmpeg acrossfade curve ids.
    /// </summary>
    public static string NormalizeCrossfadeCurve(string? curve)
    {
        if (string.IsNullOrWhiteSpace(curve))
        {
            return "tri";
        }

        var normalized = curve.Trim().ToLowerInvariant();
        if (CrossfadeCurveAliases.TryGetValue(normalized, out var aliasTarget))
        {
            return aliasTarget;
        }

        if (SupportedCrossfadeCurves.Contains(normalized))
        {
            return normalized;
        }

        var supported = string.Join(", ", SupportedCrossfadeCurves.OrderBy(value => value, StringComparer.Ordinal));
        var aliases = string.Join(", ", CrossfadeCurveAliases.Keys.OrderBy(value => value, StringComparer.Ordinal));
        throw new ArgumentOutOfRangeException(
            nameof(curve),
            curve,
            $"Crossfade curve '{curve}' is not supported. Supported FFmpeg curves: {supported}. Aliases: {aliases}.");
    }

    /// <summary>
    /// Returns a sample-accurate slice for a time range without invoking FFmpeg.
    /// </summary>
    public static AudioBuffer SliceByTime(
        AudioBuffer buffer,
        double startSec,
        double endSec,
        string? context = null)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (!double.IsFinite(startSec) || !double.IsFinite(endSec) || startSec < 0 || endSec <= startSec)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startSec),
                $"Invalid audio slice range{FormatContext(context)}: [{startSec:F6}, {endSec:F6}].");
        }

        var startSample = Math.Max(0, (int)Math.Floor(startSec * buffer.SampleRate));
        var endSample = Math.Max(startSample + 1, (int)Math.Ceiling(endSec * buffer.SampleRate));
        if (endSample > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endSec),
                $"Audio slice out of bounds{FormatContext(context)}: range=[{startSample},{endSample}), bufferLength={buffer.Length}, sampleRate={buffer.SampleRate}.");
        }

        return buffer.Slice(startSample, endSample - startSample);
    }

    /// <summary>
    /// Concatenates buffers in managed memory after validating matching sample metadata.
    /// </summary>
    public static AudioBuffer ConcatenateSegments(params AudioBuffer[] buffers)
        => ConcatenateSegments((IEnumerable<AudioBuffer>)buffers);

    /// <summary>
    /// Concatenates buffers in managed memory after validating matching sample metadata.
    /// </summary>
    public static AudioBuffer ConcatenateSegments(IEnumerable<AudioBuffer> buffers)
    {
        ArgumentNullException.ThrowIfNull(buffers);
        var materialized = buffers.ToArray();
        if (materialized.Any(buffer => buffer is null))
        {
            throw new ArgumentException("Audio segment list cannot contain null buffers.", nameof(buffers));
        }

        return AudioBuffer.Concat(materialized);
    }

    /// <summary>
    /// Clamps a requested crossfade to the supported fraction of adjacent segment durations.
    /// </summary>
    public static double ClampCrossfadeDuration(double requestedSec, double aDurationSec, double bDurationSec)
    {
        if (!double.IsFinite(requestedSec) || requestedSec < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedSec), requestedSec, "Crossfade duration must be non-negative and finite.");
        }

        if (!double.IsFinite(aDurationSec) || aDurationSec < 0 || !double.IsFinite(bDurationSec) || bDurationSec < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(aDurationSec), "Crossfade segment durations must be non-negative and finite.");
        }

        return Math.Min(requestedSec, Math.Min(aDurationSec * 0.3, bDurationSec * 0.3));
    }

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
        var normalizedCurve = NormalizeCrossfadeCurve(curve);

        // Step 1: Resample replacement if sample rates differ
        var resampled = replacement.SampleRate != original.SampleRate
            ? AudioProcessor.Resample(replacement, (ulong)original.SampleRate)
            : replacement;

        // Step 2: Trim before and after segments from original
        var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(startSec));
        var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(endSec));

        // Step 3: Clamp crossfade for first join (before + replacement)
        double clampedCrossfade1 = ClampCrossfadeDuration(
            crossfadeSec,
            DurationSeconds(before),
            DurationSeconds(resampled));

        // Step 4: Crossfade before + replacement
        var joined = Crossfade(before, resampled, clampedCrossfade1, normalizedCurve);

        // Step 5: Clamp crossfade for second join (joined + after)
        double clampedCrossfade2 = ClampCrossfadeDuration(
            crossfadeSec,
            DurationSeconds(joined),
            DurationSeconds(after));

        // Step 6: Crossfade joined + after
        return Crossfade(joined, after, clampedCrossfade2, normalizedCurve);
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

        if (!double.IsFinite(targetDurationSec))
        {
            throw new ArgumentOutOfRangeException(nameof(targetDurationSec), targetDurationSec, "Roomtone fill duration must be finite.");
        }

        if (targetDurationSec <= 0)
            return new AudioBuffer(roomtone.Channels, roomtone.SampleRate, 0, roomtone.Metadata);

        if (roomtone.Length == 0)
            throw new ArgumentException("Roomtone source buffer is empty.", nameof(roomtone));

        var targetSamples = Math.Max(1, (int)Math.Round(targetDurationSec * roomtone.SampleRate, MidpointRounding.AwayFromZero));
        if (roomtone.Length >= targetSamples)
            return roomtone.Slice(0, targetSamples);

        var result = new AudioBuffer(roomtone.Channels, roomtone.SampleRate, targetSamples, roomtone.Metadata);
        for (var channel = 0; channel < roomtone.Channels; channel++)
        {
            var source = roomtone.GetChannel(channel).Span;
            var destination = result.GetChannelSpan(channel);
            var offset = 0;
            while (offset < targetSamples)
            {
                var copyLength = Math.Min(source.Length, targetSamples - offset);
                source[..copyLength].CopyTo(destination.Slice(offset, copyLength));
                offset += copyLength;
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
        var normalizedCurve = NormalizeCrossfadeCurve(curve);

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
        double clampedCrossfade = ClampCrossfadeDuration(
            crossfadeSec,
            DurationSeconds(before),
            DurationSeconds(after));

        return Crossfade(before, after, clampedCrossfade, normalizedCurve);
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
        var normalizedCurve = NormalizeCrossfadeCurve(curve);

        // Resample insertion if sample rates differ
        var resampled = insertion.SampleRate != original.SampleRate
            ? AudioProcessor.Resample(insertion, (ulong)original.SampleRate)
            : insertion;

        var before = AudioProcessor.Trim(original, TimeSpan.Zero, TimeSpan.FromSeconds(insertPointSec));
        var after = AudioProcessor.Trim(original, TimeSpan.FromSeconds(insertPointSec));

        // Crossfade before + insertion
        double clampedCrossfade1 = ClampCrossfadeDuration(
            crossfadeSec,
            DurationSeconds(before),
            DurationSeconds(resampled));

        var joined = Crossfade(before, resampled, clampedCrossfade1, normalizedCurve);

        // Crossfade joined + after
        double clampedCrossfade2 = ClampCrossfadeDuration(
            crossfadeSec,
            DurationSeconds(joined),
            DurationSeconds(after));

        return Crossfade(joined, after, clampedCrossfade2, normalizedCurve);
    }

    /// <summary>
    /// Crossfades two audio buffers using FFmpeg's acrossfade filter.
    /// Falls back to simple concatenation when crossfade is negligible or a buffer is empty.
    /// </summary>
    private static AudioBuffer Crossfade(AudioBuffer a, AudioBuffer b, double durationSec, string curve)
    {
        // Use managed concat when crossfade is negligible or a buffer is empty.
        if (durationSec <= 0.001 || a.Length == 0 || b.Length == 0)
            return Concat(a, b);

        var inputs = new[]
        {
            new FfFilterGraphRunner.GraphInput("a", a),
            new FfFilterGraphRunner.GraphInput("b", b)
        };

        var filterSpec = FormattableString.Invariant(
            $"[a]aformat=sample_fmts=flt[af];[b]aformat=sample_fmts=flt[bf];[af][bf]acrossfade=d={durationSec:F6}:c1={curve}:c2={curve}[out]");

        return FfFilterGraphRunner.Apply(inputs, filterSpec);
    }

    private static AudioBuffer Concat(AudioBuffer a, AudioBuffer b)
    {
        if (a.Length == 0) return b;
        if (b.Length == 0) return a;
        return ConcatenateSegments(a, b);
    }

    private static string FormatContext(string? context)
        => string.IsNullOrWhiteSpace(context) ? string.Empty : $" ({context.Trim()})";
}
