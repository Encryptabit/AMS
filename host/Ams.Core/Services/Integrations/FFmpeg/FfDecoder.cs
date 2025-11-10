using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;
using static Ams.Core.Services.Integrations.FFmpeg.FfUtils;

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
            ThrowIfError(avformat_open_input(&formatContext, path, null, null), $"{nameof(avformat_open_input)}({path})");
            var tags = ReadTags(formatContext->metadata);
            ThrowIfError(avformat_find_stream_info(formatContext, null), $"{nameof(avformat_find_stream_info)}({path})");

            int streamIndex = av_find_best_stream(formatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0);
            if (streamIndex < 0)
            {
                throw new InvalidOperationException($"No audio stream found in '{path}'");
            }

            var stream = formatContext->streams[streamIndex];
            var codecParameters = stream->codecpar;
            var codecName = codecParameters != null ? avcodec_get_name(codecParameters->codec_id) : null;

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
                durationSeconds = stream->duration * av_q2d(stream->time_base);
            }
            else if (formatContext->duration != AV_NOPTS_VALUE)
            {
                durationSeconds = formatContext->duration / (double)AV_TIME_BASE;
            }
            double? startSeconds = stream->start_time != AV_NOPTS_VALUE
                ? stream->start_time * av_q2d(stream->time_base)
                : null;

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
                avformat_close_input(&formatContext);
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
            ThrowIfError(avformat_open_input(&formatContext, path, null, null), $"{nameof(avformat_open_input)}({path})");
            var container = formatContext->iformat != null
                ? PtrToStringUtf8(formatContext->iformat->name)
                : null;
            var tags = ReadTags(formatContext->metadata);
            ThrowIfError(avformat_find_stream_info(formatContext, null), $"{nameof(avformat_find_stream_info)}({path})");

            int streamIndex = av_find_best_stream(formatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0);
            if (streamIndex < 0)
            {
                throw new InvalidOperationException($"No audio stream found in '{path}'");
            }

            var stream = formatContext->streams[streamIndex];
            var codec = avcodec_find_decoder(stream->codecpar->codec_id);
            if (codec == null)
            {
                throw new InvalidOperationException($"Unsupported codec for '{path}'");
            }
            var codecName = PtrToStringUtf8(codec->name);

            AVCodecContext* codecContext = avcodec_alloc_context3(codec);
            if (codecContext == null)
            {
                throw new InvalidOperationException("Failed to allocate codec context.");
            }

            try
            {
                ThrowIfError(avcodec_parameters_to_context(codecContext, stream->codecpar), nameof(avcodec_parameters_to_context));
                ThrowIfError(avcodec_open2(codecContext, codec, null), nameof(avcodec_open2));

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
                var sourceFormatName = GetSampleFormatName((AVSampleFormat)codecContext->sample_fmt) ?? "unknown";
                var sourceLayout = CloneOrDefault(&codecContext->ch_layout, sourceChannels);
                AVChannelLayout? targetLayout = null;
                SwrContext* resampler = null;

                var needsResample = targetSampleRate != sourceSampleRate
                                    || targetChannels != sourceChannels
                                    || (AVSampleFormat)codecContext->sample_fmt != targetFormat;

                if (needsResample)
                {
                    targetLayout = CreateDefaultChannelLayout(targetChannels);
                    resampler = swr_alloc();
                    if (resampler == null)
                    {
                        throw new InvalidOperationException("Failed to allocate FFmpeg resampler.");
                    }

                    var layoutValue = targetLayout.Value;
                    var resamplerPtr = resampler;
                    var configureResult = swr_alloc_set_opts2(
                        &resamplerPtr,
                        &layoutValue,
                        targetFormat,
                        targetSampleRate,
                        &sourceLayout,
                        (AVSampleFormat)codecContext->sample_fmt,
                        sourceSampleRate,
                        0,
                        null);
                    ThrowIfError(configureResult, nameof(swr_alloc_set_opts2));
                    resampler = resamplerPtr;

                    ThrowIfError(swr_init(resampler), nameof(swr_init));
                }

                try
                {
                    using var packet = new FfPacket();
                    using var frame = new FfFrame();

                    var channelSamples = new List<List<float>>(targetChannels);
                    for (int i = 0; i < targetChannels; i++)
                    {
                        channelSamples.Add(new List<float>(65536));
                    }

                    while (true)
                    {
                        var readResult = av_read_frame(formatContext, packet.Pointer);
                        if (readResult == AVERROR_EOF)
                        {
                            break;
                        }

                        ThrowIfError(readResult, nameof(av_read_frame));

                        if (packet.Pointer->stream_index != streamIndex)
                        {
                            packet.Unref();
                            continue;
                        }

                        ThrowIfError(avcodec_send_packet(codecContext, packet.Pointer), nameof(avcodec_send_packet));
                        packet.Unref();

                        while (true)
                        {
                            var receive = avcodec_receive_frame(codecContext, frame.Pointer);
                            if (receive == AVERROR(EAGAIN) || receive == AVERROR_EOF)
                            {
                                break;
                            }

                            ThrowIfError(receive, nameof(avcodec_receive_frame));
                            AppendSamples(frame.Pointer, resampler, needsResample, sourceSampleRate, targetChannels, targetSampleRate, targetFormat, channelSamples);
                        }
                    }

                    // Flush decoder
                    ThrowIfError(avcodec_send_packet(codecContext, null), nameof(avcodec_send_packet));
                    while (true)
                    {
                        var receive = avcodec_receive_frame(codecContext, frame.Pointer);
                        if (receive == AVERROR(EAGAIN) || receive == AVERROR_EOF)
                        {
                            break;
                        }

                        ThrowIfError(receive, nameof(avcodec_receive_frame));
                        AppendSamples(frame.Pointer, resampler, needsResample, sourceSampleRate, targetChannels, targetSampleRate, targetFormat, channelSamples);
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

                    var durationSeconds = stream->duration > 0 && stream->time_base.den != 0
                        ? stream->duration * av_q2d(stream->time_base)
                        : (formatContext->duration != AV_NOPTS_VALUE ? formatContext->duration / (double)AV_TIME_BASE : 0);
                    double? startSeconds = stream->start_time != AV_NOPTS_VALUE
                        ? stream->start_time * av_q2d(stream->time_base)
                        : null;
                    var metadata = new AudioBufferMetadata
                    {
                        SourcePath = Path.GetFullPath(path),
                        ContainerFormat = container,
                        CodecName = codecName,
                        SourceDurationSeconds = durationSeconds,
                        SourceStartSeconds = startSeconds,
                        SourceSampleRate = sourceSampleRate,
                        CurrentSampleRate = targetSampleRate,
                        SourceChannels = sourceChannels,
                        CurrentChannels = targetChannels,
                        SourceSampleFormat = sourceFormatName,
                        CurrentSampleFormat = "fltp",
                        SourceChannelLayout = AudioBufferMetadata.DescribeDefaultLayout(sourceChannels),
                        CurrentChannelLayout = AudioBufferMetadata.DescribeDefaultLayout(targetChannels),
                        Tags = tags
                    };
                    buffer.UpdateMetadata(metadata);
                    return buffer;
                }
                finally
                {
                    if (resampler != null)
                    {
                        swr_free(&resampler);
                    }

                    av_channel_layout_uninit(&sourceLayout);
                    if (targetLayout.HasValue)
                    {
                        var layout = targetLayout.Value;
                        av_channel_layout_uninit(&layout);
                    }
                }
            }
            finally
            {
                avcodec_free_context(&codecContext);
            }
        }
        finally
        {
            if (formatContext != null)
            {
                avformat_close_input(&formatContext);
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
        var dstNbSamples = ComputeResampleOutputSamples(resampler, sourceSampleRate, targetSampleRate, frame->nb_samples);

        byte** converted = null;
        try
        {
            var ret = av_samples_alloc_array_and_samples(&converted, null, targetChannels, dstNbSamples, targetFormat, 0);
            ThrowIfError(ret, nameof(av_samples_alloc_array_and_samples));

            var samples = swr_convert(resampler, converted, dstNbSamples, frame->extended_data, frame->nb_samples);
            ThrowIfError(samples, nameof(swr_convert));

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
                av_freep(&converted[0]);
                av_free(converted);
            }
        }
    }

    private static unsafe void AppendSamples(
        AVFrame* frame,
        SwrContext* resampler,
        bool needsResample,
        int sourceSampleRate,
        int targetChannels,
        int targetSampleRate,
        AVSampleFormat targetFormat,
        IList<List<float>> channelSamples)
    {
        if (needsResample)
        {
            ResampleInto(resampler, sourceSampleRate, frame, targetChannels, targetSampleRate, targetFormat, channelSamples);
            return;
        }

        var samples = frame->nb_samples;
        for (int ch = 0; ch < targetChannels; ch++)
        {
            var source = (float*)frame->extended_data[ch];
            var list = channelSamples[ch];
            for (int i = 0; i < samples; i++)
            {
                list.Add(source[i]);
            }
        }
    }

    private static string? GetSampleFormatName(AVSampleFormat format)
    {
        var name = ffmpeg.av_get_sample_fmt_name(format);
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private static IReadOnlyDictionary<string, string>? ReadTags(AVDictionary* metadata)
    {
        if (metadata == null)
        {
            return null;
        }

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AVDictionaryEntry* entry = null;
        while ((entry = ffmpeg.av_dict_get(metadata, string.Empty, entry, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
        {
            var key = Marshal.PtrToStringUTF8((nint)entry->key);
            var value = Marshal.PtrToStringUTF8((nint)entry->value);
            if (!string.IsNullOrWhiteSpace(key))
            {
                dict[key] = value ?? string.Empty;
            }
        }

        return dict.Count == 0 ? null : dict;
    }

    private sealed class FfPacket : IDisposable
    {
        private AVPacket* _pointer;
        public AVPacket* Pointer => _pointer;

        public FfPacket()
        {
            _pointer = av_packet_alloc();
            if (_pointer == null)
            {
                throw new InvalidOperationException("Failed to allocate packet.");
            }
        }

        public void Unref()
        {
            if (_pointer != null)
            {
                av_packet_unref(_pointer);
            }
        }

        public void Dispose()
        {
            if (_pointer != null)
            {
                var local = _pointer;
                av_packet_free(&local);
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
            _pointer = av_frame_alloc();
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
                av_frame_free(&local);
                _pointer = null;
            }
        }
    }
}
