using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Core.Audio;

/// <summary>
/// Prepares audio buffers for ASR engines (Whisper, Nemo) by converting to mono 16kHz.
/// </summary>
/// <remarks>
/// <para>
/// ASR engines require mono 16kHz audio for optimal performance. This utility provides
/// a unified implementation that handles channel downmixing and sample rate conversion.
/// </para>
/// <para>
/// <b>FFmpeg path (high quality):</b> When FFmpeg filter graphs are available, uses the
/// pan filter for proper channel mixing with equal weighting. This preserves audio quality
/// better than simple averaging.
/// </para>
/// <para>
/// <b>Fallback path:</b> When FFmpeg is unavailable, uses simple per-sample averaging
/// for downmixing. This is less accurate but works in all environments.
/// </para>
/// </remarks>
public static class AsrAudioPreparer
{
    /// <summary>
    /// Prepares an audio buffer for ASR by converting to mono and resampling to 16kHz.
    /// Uses FFmpeg filter graph when available for high-quality conversion,
    /// falls back to simple averaging otherwise.
    /// </summary>
    /// <param name="buffer">The source audio buffer.</param>
    /// <returns>A mono 16kHz buffer ready for ASR processing.</returns>
    public static AudioBuffer PrepareForAsr(AudioBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        // Already ASR-ready
        if (buffer.Channels == 1 && buffer.SampleRate == AudioProcessor.DefaultAsrSampleRate)
        {
            return buffer;
        }

        var working = buffer;

        // Step 1: Downmix to mono if needed
        if (working.Channels != 1)
        {
            working = DownmixToMono(working);
        }

        // Step 2: Resample to 16kHz if needed
        if (working.SampleRate != AudioProcessor.DefaultAsrSampleRate)
        {
            working = AudioProcessor.Resample(working, AudioProcessor.DefaultAsrSampleRate);
        }

        return working;
    }

    /// <summary>
    /// Downmixes a multi-channel buffer to mono.
    /// Uses FFmpeg pan filter for high-quality mixing when available,
    /// falls back to simple per-sample averaging otherwise.
    /// </summary>
    private static AudioBuffer DownmixToMono(AudioBuffer buffer)
    {
        if (buffer.Channels == 1)
        {
            return buffer;
        }

        // Try FFmpeg path first for higher quality
        if (FfSession.FiltersAvailable)
        {
            return FfFilterGraph
                .FromBuffer(buffer)
                .Custom(BuildMonoPanClause(buffer.Channels))
                .ToBuffer();
        }

        // Fallback: simple averaging
        return DownmixToMonoSimple(buffer);
    }

    /// <summary>
    /// Builds an FFmpeg pan filter clause for equal-weighted mono downmix.
    /// </summary>
    /// <param name="channels">The number of source channels.</param>
    /// <returns>An FFmpeg pan filter clause string.</returns>
    /// <example>
    /// For 2 channels: "pan=mono|c0=0.500000*c0+0.500000*c1"
    /// For 6 channels: "pan=mono|c0=0.166667*c0+0.166667*c1+...+0.166667*c5"
    /// </example>
    internal static string BuildMonoPanClause(int channels)
    {
        if (channels <= 1)
        {
            return "pan=mono|c0=c0";
        }

        var weight = 1.0 / channels;
        var builder = new StringBuilder();
        for (var ch = 0; ch < channels; ch++)
        {
            if (ch > 0)
            {
                builder.Append('+');
            }

            builder.Append(FormattableString.Invariant($"{weight:F6}*c{ch}"));
        }

        return $"pan=mono|c0={builder}";
    }

    /// <summary>
    /// Simple per-sample averaging downmix (fallback when FFmpeg unavailable).
    /// </summary>
    private static AudioBuffer DownmixToMonoSimple(AudioBuffer buffer)
    {
        if (buffer.Channels == 1)
        {
            return buffer;
        }

        var mono = new AudioBuffer(1, buffer.SampleRate, buffer.Length);
        for (var i = 0; i < buffer.Length; i++)
        {
            double sum = 0;
            for (var ch = 0; ch < buffer.Channels; ch++)
            {
                sum += buffer.Planar[ch][i];
            }

            mono.Planar[0][i] = (float)(sum / buffer.Channels);
        }

        return mono;
    }
}
