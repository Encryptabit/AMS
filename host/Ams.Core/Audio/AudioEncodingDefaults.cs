using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Core.Audio;

internal static class AudioEncodingDefaults
{
    public static AudioEncodeOptions ForSource(AudioBuffer sourceBuffer)
    {
        ArgumentNullException.ThrowIfNull(sourceBuffer);

        return new AudioEncodeOptions(
            TargetSampleRate: sourceBuffer.SampleRate,
            TargetBitDepth: ResolvePreferredBitDepth(sourceBuffer),
            TargetSampleEncoding: ResolvePreferredSampleEncoding(sourceBuffer));
    }

    public static int ResolvePreferredBitDepth(AudioBuffer sourceBuffer)
    {
        ArgumentNullException.ThrowIfNull(sourceBuffer);

        var codecName = sourceBuffer.Metadata.CodecName;
        if (TryResolvePcmBitDepth(codecName, out var pcmBitDepth))
        {
            return pcmBitDepth;
        }

        return 16;
    }

    public static AudioSampleEncoding? ResolvePreferredSampleEncoding(AudioBuffer sourceBuffer)
    {
        ArgumentNullException.ThrowIfNull(sourceBuffer);

        var codecName = sourceBuffer.Metadata.CodecName;
        if (string.IsNullOrWhiteSpace(codecName))
        {
            return null;
        }

        var normalized = codecName.Trim().ToLowerInvariant();
        if (normalized.Contains("pcm_f32", StringComparison.Ordinal))
        {
            return AudioSampleEncoding.Float;
        }

        if (normalized.Contains("pcm_s", StringComparison.Ordinal))
        {
            return AudioSampleEncoding.SignedInteger;
        }

        return null;
    }

    private static bool TryResolvePcmBitDepth(string? codecName, out int bitDepth)
    {
        bitDepth = default;
        if (string.IsNullOrWhiteSpace(codecName))
        {
            return false;
        }

        var normalized = codecName.Trim().ToLowerInvariant();
        if (normalized.Contains("pcm_s24", StringComparison.Ordinal))
        {
            bitDepth = 24;
            return true;
        }

        if (normalized.Contains("pcm_s16", StringComparison.Ordinal))
        {
            bitDepth = 16;
            return true;
        }

        if (normalized.Contains("pcm_f32", StringComparison.Ordinal) ||
            normalized.Contains("pcm_s32", StringComparison.Ordinal))
        {
            bitDepth = 32;
            return true;
        }

        return false;
    }
}
