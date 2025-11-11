using System;
using System.Globalization;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace Ams.Core.Services.Integrations.FFmpeg;

internal static unsafe class FfUtils
{
    private const int ErrorBufferSize = 1024;

    public static unsafe void ThrowIfError(int errorCode, string where)
    {
        if (errorCode >= 0)
        {
            return;
        }

        var message = FormatError(errorCode);
        throw new InvalidOperationException(
            $"ffmpeg: {where} failed: {message} ({errorCode})");
    }

    public static unsafe string FormatError(int errorCode)
    {
        var buffer = stackalloc byte[ErrorBufferSize];
        av_strerror(errorCode, buffer, (ulong)ErrorBufferSize);
        return Marshal.PtrToStringAnsi((IntPtr)buffer) ?? errorCode.ToString(CultureInfo.InvariantCulture);
    }

    public static void CleanupThrowIfError(string message, AVFormatContext* fmt, AVCodecContext* cc, AVIOContext* avio,
        GCHandle handle)
    {
        if (avio != null)
        {
            av_freep(&avio->buffer);
            avio_context_free(&avio);
        }

        if (cc != null)
        {
            av_channel_layout_uninit(&cc->ch_layout);
            avcodec_free_context(&cc);
        }

        if (fmt != null) avformat_free_context(fmt);
        if (handle.IsAllocated) handle.Free();
        throw new InvalidOperationException(message);
    }

    public static unsafe AVChannelLayout CloneOrDefault(AVChannelLayout* layout, int fallbackChannels)
    {
        AVChannelLayout result = default;
        if (layout != null && layout->nb_channels > 0)
        {
            ThrowIfError(av_channel_layout_copy(&result, layout), "Failed to copy channel layout");
            return result;
        }

        av_channel_layout_default(&result, fallbackChannels);
        return result;
    }

    public static unsafe AVChannelLayout CreateDefaultChannelLayout(int channels)
    {
        AVChannelLayout layout = default;
        av_channel_layout_default(&layout, channels);
        return layout;
    }

    public static unsafe int CheckSampleFormat(AVCodec* codec, AVSampleFormat format)
    {
        if (codec is null)
        {
            throw new ArgumentNullException(nameof(codec));
        }

        if (format == AVSampleFormat.AV_SAMPLE_FMT_NONE)
        {
            throw new ArgumentException("Invalid sample format", nameof(format));
        }

        // TODO: migrate to avcodec_get_supported_config once we need runtime negotiation.
        return 0;
    }

    public static unsafe int SelectSampleRate(AVCodecContext* ctx)
    {
        int* supportedSamplerates;
        int i, numSamplerates, ret;

        ret = avcodec_get_supported_config(ctx, null, AVCodecConfig.AV_CODEC_CONFIG_SAMPLE_RATE,
            0, (void**)&supportedSamplerates, &numSamplerates);

        // return error
        if (ret < 0) return ret;

        if (supportedSamplerates is null)
        {
            return 44100;
        }

        for (i = 0; i < numSamplerates; i++)
        {
            if (ctx->sample_rate == supportedSamplerates[i])
            {
                return supportedSamplerates[i];
            }

            if (i == numSamplerates)
            {
                av_log(ctx, AV_LOG_ERROR,
                    $"Specified sample rate {ctx->sample_rate} is not supported by the {ctx->codec->name->ToString()} encoder\n");
            }
        }

        return 0;
    }

    public static unsafe AVChannelLayout SelectChannelLayout(AVCodecContext* ctx)
    {
        if (ctx == null)
            throw new ArgumentNullException(nameof(ctx));

        var channels = ctx->ch_layout.nb_channels > 0 ? ctx->ch_layout.nb_channels : 1;
        return CreateDefaultChannelLayout(channels);
    }

    public static unsafe int ComputeResampleOutputSamples(SwrContext* resampler, int sourceSampleRate,
        int targetSampleRate, int sourceSamples)
    {
        var delay = swr_get_delay(resampler, sourceSampleRate);
        return (int)av_rescale_rnd(delay + sourceSamples, targetSampleRate, sourceSampleRate, AVRounding.AV_ROUND_UP);
    }

    public static string FormatNumber(double value, string format = "0.####")
        => value.ToString(format, CultureInfo.InvariantCulture);

    public static string FormatDecibels(double value)
        => $"{FormatNumber(value)}dB";

    public static string FormatMilliseconds(double value)
        => FormatNumber(value);

    public static string FormatFraction(double value, double min = 0d, double max = 1d)
    {
        var clamped = Math.Clamp(value, min, max);
        return FormatNumber(clamped);
    }
}
