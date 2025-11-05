using System;
using System.Globalization;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg;

internal static class FfUtils
{
    private const int ErrorBufferSize = 1024;

    public static unsafe void ThrowIfError(int errorCode, string message)
    {
        if (errorCode >= 0)
        {
            return;
        }

        var error = GetErrorMessage(errorCode);
        throw new InvalidOperationException($"{message} (ffmpeg: {error})");
    }

    public static unsafe string GetErrorMessage(int errorCode)
    {
        var buffer = stackalloc byte[ErrorBufferSize];
        ffmpeg.av_strerror(errorCode, buffer, (ulong)ErrorBufferSize);
        var result = Marshal.PtrToStringAnsi((nint)buffer);
        return string.IsNullOrEmpty(result)
            ? errorCode.ToString(CultureInfo.InvariantCulture)
            : result!;
    }

    public static unsafe AVChannelLayout CloneOrDefault(AVChannelLayout* layout, int fallbackChannels)
    {
        AVChannelLayout result = default;
        if (layout != null && layout->nb_channels > 0)
        {
            ThrowIfError(ffmpeg.av_channel_layout_copy(&result, layout), "Failed to copy channel layout");
            return result;
        }

        ffmpeg.av_channel_layout_default(&result, fallbackChannels);
        return result;
    }

    public static unsafe AVChannelLayout CreateDefaultChannelLayout(int channels)
    {
        AVChannelLayout layout = default;
        ffmpeg.av_channel_layout_default(&layout, channels);
        return layout;
    }
}
