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

    public static unsafe int CheckSampleFormat(AVCodec* codec, AVSampleFormat format)
    {
        AVSampleFormat* p = codec->sample_fmts;
        if (format == AVSampleFormat.AV_SAMPLE_FMT_NONE)
        {
            throw new ArgumentException("Invalid sample format", nameof(format));
        }

        return 0;
    }

    public static unsafe int SelectSampleRate(AVCodecContext* ctx)
    {
        int* supportedSamplerates;
        int i, numSamplerates, ret;
        
        ret = ffmpeg.avcodec_get_supported_config(ctx, null, AVCodecConfig.AV_CODEC_CONFIG_SAMPLE_RATE,
            0, (void**)&supportedSamplerates, &numSamplerates);

        // return error
        if (ret < 0) return ret;
        
        if (supportedSamplerates is null)
        {
            return 44100;
        }

        for(i = 0; i < numSamplerates; i++)
        {
            if(ctx->sample_rate == supportedSamplerates[i])
            {
                return supportedSamplerates[i];
            }

            if (i == numSamplerates)
            {
                ffmpeg.av_log(ctx, ffmpeg.AV_LOG_ERROR, $"Specified sample rate {ctx->sample_rate} is not supported by the {ctx->codec->name->ToString()} encoder\n");
            } 

        }

        return 0;
    }

    public static unsafe AVChannelLayout SelectChannelLayout(AVCodecContext* ctx)
    {
        
        // short circuit for now, we dont have to support this yet
        return CreateDefaultChannelLayout(1); 
        
        ulong* p;
        AVChannelLayout* chLayouts;
        int numChLayouts = 0;
        
        ffmpeg.avcodec_get_supported_config(ctx, null, AVCodecConfig.AV_CODEC_CONFIG_CHANNEL_LAYOUT,
            0, (void**)&chLayouts, &numChLayouts);

    }
}