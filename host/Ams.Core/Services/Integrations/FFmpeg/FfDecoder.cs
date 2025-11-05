using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg;

internal static unsafe class FfDecoder
{
    public static AudioInfo Probe(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Audio file not found: {path}", path);
        }

        FfSession.EnsureInitialized();

        AVFormatContext* formatContext = null;
        try
        {
            FfUtils.ThrowIfError(ffmpeg.avformat_open_input(&formatContext, path, null, null), $"Failed to open '{path}'");
            FfUtils.ThrowIfError(ffmpeg.avformat_find_stream_info(formatContext, null), $"Failed to determine stream info for '{path}'");

            int streamIndex = ffmpeg.av_find_best_stream(formatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0);
            if (streamIndex < 0)
            {
                throw new InvalidOperationException($"No audio stream found in '{path}'");
            }

            var stream = formatContext->streams[streamIndex];
            var codecParameters = stream->codecpar;

            var container = formatContext->iformat != null
                ? PtrToStringUtf8(formatContext->iformat->name)
                : "unknown";

            int sampleRate = codecParameters->sample_rate;
            int channels = codecParameters->ch_layout.nb_channels;
            if (channels <= 0)
            {
                channels = 1;
            }

            double durationSeconds = 0;
            if (stream->duration > 0 && stream->time_base.den != 0)
            {
                durationSeconds = stream->duration * ffmpeg.av_q2d(stream->time_base);
            }
            else if (formatContext->duration != ffmpeg.AV_NOPTS_VALUE)
            {
                durationSeconds = formatContext->duration / (double)ffmpeg.AV_TIME_BASE;
            }

            return new AudioInfo(
                container,
                sampleRate,
                channels,
                TimeSpan.FromSeconds(Math.Max(0, durationSeconds)));
        }
        finally
        {
            if (formatContext != null)
            {
                ffmpeg.avformat_close_input(&formatContext);
            }
        }
    }

    private static string PtrToStringUtf8(byte* value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return Marshal.PtrToStringUTF8((nint)value) ?? string.Empty;
    }

    public static AudioBuffer Decode(string path, AudioDecodeOptions options)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Audio file not found: {path}", path);
        }

        FfSession.EnsureInitialized();

        if (options.Start.HasValue || options.Duration.HasValue)
        {
            throw new NotSupportedException("Time range decoding is not implemented yet.");
        }

        AVFormatContext* formatContext = null;
        try
        {
            FfUtils.ThrowIfError(ffmpeg.avformat_open_input(&formatContext, path, null, null), $"Failed to open '{path}'");
            FfUtils.ThrowIfError(ffmpeg.avformat_find_stream_info(formatContext, null), $"Failed to determine stream info for '{path}'");

            int streamIndex = ffmpeg.av_find_best_stream(formatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0);
            if (streamIndex < 0)
            {
                throw new InvalidOperationException($"No audio stream found in '{path}'");
            }

            var stream = formatContext->streams[streamIndex];
            var codec = ffmpeg.avcodec_find_decoder(stream->codecpar->codec_id);
            if (codec == null)
            {
                throw new InvalidOperationException($"Unsupported codec for '{path}'");
            }

            AVCodecContext* codecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (codecContext == null)
            {
                throw new InvalidOperationException("Failed to allocate codec context.");
            }

            try
            {
                FfUtils.ThrowIfError(ffmpeg.avcodec_parameters_to_context(codecContext, stream->codecpar), "Failed to copy codec parameters");
                FfUtils.ThrowIfError(ffmpeg.avcodec_open2(codecContext, codec, null), "Failed to open codec");

                var sourceSampleRate = codecContext->sample_rate;
                if (sourceSampleRate <= 0)
                {
                    sourceSampleRate = stream->codecpar->sample_rate;
                }
                if (sourceSampleRate <= 0)
                {
                    sourceSampleRate = AudioProcessor.DefaultAsrSampleRate;
                }
                var sourceChannels = codecContext->ch_layout.nb_channels;
                if (sourceChannels <= 0)
                {
                    sourceChannels = stream->codecpar->ch_layout.nb_channels;
                }
                if (sourceChannels <= 0)
                {
                    sourceChannels = 1;
                }

                var targetSampleRate = options.TargetSampleRate ?? sourceSampleRate;
                var targetChannels = options.TargetChannels ?? sourceChannels;
                if (targetChannels <= 0)
                {
                    throw new InvalidOperationException("Target channels must be positive.");
                }

                var targetFormat = AVSampleFormat.AV_SAMPLE_FMT_FLTP;
                var sourceLayout = FfUtils.CloneOrDefault(&codecContext->ch_layout, sourceChannels);
                var targetLayout = FfUtils.CreateDefaultChannelLayout(targetChannels);

                SwrContext* resampler = ffmpeg.swr_alloc();
                if (resampler == null)
                {
                    throw new InvalidOperationException("Failed to allocate FFmpeg resampler.");
                }

                try
                {
                    var resamplerPtr = resampler;
                    var configureResult = ffmpeg.swr_alloc_set_opts2(
                        &resamplerPtr,
                        &targetLayout,
                        targetFormat,
                        targetSampleRate,
                        &sourceLayout,
                        (AVSampleFormat)codecContext->sample_fmt,
                        sourceSampleRate,
                        0,
                        null);
                    FfUtils.ThrowIfError(configureResult, "Failed to configure resampler");
                    resampler = resamplerPtr;

                    FfUtils.ThrowIfError(ffmpeg.swr_init(resampler), "Failed to initialize resampler");

                    using var packet = new FfPacket();
                    using var frame = new FfFrame();

                    var channelSamples = new List<List<float>>(targetChannels);
                    for (int i = 0; i < targetChannels; i++)
                    {
                        channelSamples.Add(new List<float>(65536));
                    }

                    while (true)
                    {
                        var readResult = ffmpeg.av_read_frame(formatContext, packet.Pointer);
                        if (readResult == ffmpeg.AVERROR_EOF)
                        {
                            break;
                        }

                        FfUtils.ThrowIfError(readResult, "Failed to read audio frame");

                        if (packet.Pointer->stream_index != streamIndex)
                        {
                            packet.Unref();
                            continue;
                        }

                        FfUtils.ThrowIfError(ffmpeg.avcodec_send_packet(codecContext, packet.Pointer), "Failed to send packet for decoding");
                        packet.Unref();

                        while (true)
                        {
                            var receive = ffmpeg.avcodec_receive_frame(codecContext, frame.Pointer);
                            if (receive == ffmpeg.AVERROR(ffmpeg.EAGAIN) || receive == ffmpeg.AVERROR_EOF)
                            {
                                break;
                            }

                            FfUtils.ThrowIfError(receive, "Failed to receive decoded frame");
                            ResampleInto(resampler, sourceSampleRate, frame.Pointer, targetChannels, targetSampleRate, targetFormat, channelSamples);
                        }
                    }

                    // Flush decoder
                    FfUtils.ThrowIfError(ffmpeg.avcodec_send_packet(codecContext, null), "Failed to flush decoder");
                    while (true)
                    {
                        var receive = ffmpeg.avcodec_receive_frame(codecContext, frame.Pointer);
                        if (receive == ffmpeg.AVERROR(ffmpeg.EAGAIN) || receive == ffmpeg.AVERROR_EOF)
                        {
                            break;
                        }

                        FfUtils.ThrowIfError(receive, "Failed to receive decoded frame during flush");
                        ResampleInto(resampler, sourceSampleRate, frame.Pointer, targetChannels, targetSampleRate, targetFormat, channelSamples);
                    }

                    var length = channelSamples.Count > 0 ? channelSamples[0].Count : 0;
                    var buffer = new AudioBuffer(targetChannels, targetSampleRate, length);
                    for (int ch = 0; ch < targetChannels; ch++)
                    {
                        if (channelSamples[ch].Count != length)
                        {
                            throw new InvalidOperationException("Channel sample count mismatch during decode.");
                        }

                        channelSamples[ch].CopyTo(buffer.Planar[ch], 0);
                    }

                    return buffer;
                }
                finally
                {
                    ffmpeg.swr_free(&resampler);
                    ffmpeg.av_channel_layout_uninit(&sourceLayout);
                    ffmpeg.av_channel_layout_uninit(&targetLayout);
                }
            }
            finally
            {
                ffmpeg.avcodec_free_context(&codecContext);
            }
        }
        finally
        {
            if (formatContext != null)
            {
                ffmpeg.avformat_close_input(&formatContext);
            }
        }
    }

    private static void ResampleInto(
        SwrContext* resampler,
        int sourceSampleRate,
        AVFrame* frame,
        int targetChannels,
        int targetSampleRate,
        AVSampleFormat targetFormat,
        IList<List<float>> channelSamples)
    {
        var delay = ffmpeg.swr_get_delay(resampler, sourceSampleRate);
        var dstNbSamples = (int)ffmpeg.av_rescale_rnd(
            delay + frame->nb_samples,
            targetSampleRate,
            sourceSampleRate,
            AVRounding.AV_ROUND_UP);

        byte** converted = null;
        try
        {
            var ret = ffmpeg.av_samples_alloc_array_and_samples(&converted, null, targetChannels, dstNbSamples, targetFormat, 0);
            FfUtils.ThrowIfError(ret, "Failed to allocate resample buffer");

            var samples = ffmpeg.swr_convert(resampler, converted, dstNbSamples, frame->extended_data, frame->nb_samples);
            FfUtils.ThrowIfError(samples, "Failed to convert samples");

            for (int ch = 0; ch < targetChannels; ch++)
            {
                var dst = (float*)converted[ch];
                var list = channelSamples[ch];
                for (int i = 0; i < samples; i++)
                {
                    list.Add(dst[i]);
                }
            }
        }
        finally
        {
            if (converted != null)
            {
                ffmpeg.av_freep(&converted[0]);
                ffmpeg.av_free(converted);
            }
        }
    }

    private sealed class FfPacket : IDisposable
    {
        private AVPacket* _pointer;
        public AVPacket* Pointer => _pointer;

        public FfPacket()
        {
            _pointer = ffmpeg.av_packet_alloc();
            if (_pointer == null)
            {
                throw new InvalidOperationException("Failed to allocate packet.");
            }
        }

        public void Unref()
        {
            if (_pointer != null)
            {
                ffmpeg.av_packet_unref(_pointer);
            }
        }

        public void Dispose()
        {
            if (_pointer != null)
            {
                var local = _pointer;
                ffmpeg.av_packet_free(&local);
                _pointer = null;
            }
        }
    }

    private sealed class FfFrame : IDisposable
    {
        private AVFrame* _pointer;
        public AVFrame* Pointer => _pointer;

        public FfFrame()
        {
            _pointer = ffmpeg.av_frame_alloc();
            if (_pointer == null)
            {
                throw new InvalidOperationException("Failed to allocate frame.");
            }
        }

        public void Dispose()
        {
            if (_pointer != null)
            {
                var local = _pointer;
                ffmpeg.av_frame_free(&local);
                _pointer = null;
            }
        }
    }
}
