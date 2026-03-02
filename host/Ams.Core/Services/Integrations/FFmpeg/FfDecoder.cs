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
            ThrowIfError(avformat_open_input(&formatContext, path, null, null),
                $"{nameof(avformat_open_input)}({path})");
            var tags = ReadTags(formatContext->metadata);
            ThrowIfError(avformat_find_stream_info(formatContext, null),
                $"{nameof(avformat_find_stream_info)}({path})");

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

            // Extract bit depth from codec parameters.
            // PCM codecs expose bits_per_raw_sample (e.g. 24 for pcm_s24le, 16 for pcm_s16le).
            // Fall back to bits_per_coded_sample, then infer from sample format.
            int bitsPerSample = codecParameters->bits_per_raw_sample;
            if (bitsPerSample <= 0)
                bitsPerSample = codecParameters->bits_per_coded_sample;
            if (bitsPerSample <= 0)
            {
                bitsPerSample = (AVSampleFormat)codecParameters->format switch
                {
                    AVSampleFormat.AV_SAMPLE_FMT_U8 or AVSampleFormat.AV_SAMPLE_FMT_U8P => 8,
                    AVSampleFormat.AV_SAMPLE_FMT_S16 or AVSampleFormat.AV_SAMPLE_FMT_S16P => 16,
                    AVSampleFormat.AV_SAMPLE_FMT_S32 or AVSampleFormat.AV_SAMPLE_FMT_S32P => 32,
                    AVSampleFormat.AV_SAMPLE_FMT_FLT or AVSampleFormat.AV_SAMPLE_FMT_FLTP => 32,
                    AVSampleFormat.AV_SAMPLE_FMT_DBL or AVSampleFormat.AV_SAMPLE_FMT_DBLP => 64,
                    AVSampleFormat.AV_SAMPLE_FMT_S64 or AVSampleFormat.AV_SAMPLE_FMT_S64P => 64,
                    _ => 0,
                };
            }

            return new AudioInfo(
                container,
                sampleRate,
                channels,
                TimeSpan.FromSeconds(Math.Max(0, durationSeconds)),
                bitsPerSample);
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

        AVFormatContext* formatContext = null;
        try
        {
            ThrowIfError(avformat_open_input(&formatContext, path, null, null),
                $"{nameof(avformat_open_input)}({path})");
            var container = formatContext->iformat != null
                ? PtrToStringUtf8(formatContext->iformat->name)
                : null;
            var tags = ReadTags(formatContext->metadata);
            ThrowIfError(avformat_find_stream_info(formatContext, null),
                $"{nameof(avformat_find_stream_info)}({path})");

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
                ThrowIfError(avcodec_parameters_to_context(codecContext, stream->codecpar),
                    nameof(avcodec_parameters_to_context));
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
                    using var resampleScratch = new ResampleScratch();

                    var channelSamples = new List<List<float>>(targetChannels);
                    for (int i = 0; i < targetChannels; i++)
                    {
                        channelSamples.Add(new List<float>(65536));
                    }

                    // Time-range optimization: seek forward and limit decode window
                    // to avoid decoding/buffering the entire file for partial loads.
                    bool hasTimeRange = options.Start.HasValue || options.Duration.HasValue;
                    double firstDecodedTimeSec = 0;
                    bool firstFrameSeen = false;
                    long maxDecodeSamples = long.MaxValue;

                    if (hasTimeRange && options.Start.HasValue)
                    {
                        long seekTs = (long)(options.Start.Value.TotalSeconds * AV_TIME_BASE);
                        av_seek_frame(formatContext, -1, seekTs, AVSEEK_FLAG_BACKWARD);
                        avcodec_flush_buffers(codecContext);
                    }

                    if (hasTimeRange && options.Duration.HasValue)
                    {
                        // Generous margin (10s) for keyframe distance after seek
                        maxDecodeSamples = (long)((options.Duration.Value.TotalSeconds + 10.0) * targetSampleRate);
                    }

                    bool decodeLimitReached = false;

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

                            // Track first decoded frame PTS for precise trimming after seek
                            if (hasTimeRange && !firstFrameSeen && frame.Pointer->pts != AV_NOPTS_VALUE)
                            {
                                firstDecodedTimeSec = frame.Pointer->pts * av_q2d(stream->time_base);
                                firstFrameSeen = true;
                            }

                            AppendSamples(frame.Pointer, resampler, needsResample, sourceSampleRate, targetChannels,
                                targetSampleRate, targetFormat, channelSamples, resampleScratch);

                            // Early exit once we have enough samples for the requested range
                            if (channelSamples[0].Count >= maxDecodeSamples)
                            {
                                decodeLimitReached = true;
                                break;
                            }
                        }

                        if (decodeLimitReached) break;
                    }

                    // Flush decoder (skip if we already have enough samples)
                    if (!decodeLimitReached)
                    {
                        ThrowIfError(avcodec_send_packet(codecContext, null), nameof(avcodec_send_packet));
                        while (true)
                        {
                            var receive = avcodec_receive_frame(codecContext, frame.Pointer);
                            if (receive == AVERROR(EAGAIN) || receive == AVERROR_EOF)
                            {
                                break;
                            }

                            ThrowIfError(receive, nameof(avcodec_receive_frame));
                            AppendSamples(frame.Pointer, resampler, needsResample, sourceSampleRate, targetChannels,
                                targetSampleRate, targetFormat, channelSamples, resampleScratch);
                        }
                    }

                    // Precise trim: after seeking we may have decoded a small window
                    // before the requested start; trim to exact boundaries.
                    if (hasTimeRange && channelSamples.Count > 0 && channelSamples[0].Count > 0)
                    {
                        var totalDecoded = channelSamples[0].Count;
                        var requestedStartSec = options.Start?.TotalSeconds ?? 0;
                        var offsetFromFirst = Math.Max(0, requestedStartSec - firstDecodedTimeSec);
                        var sliceStart = Math.Clamp((int)(offsetFromFirst * targetSampleRate), 0, totalDecoded);
                        var sliceCount = options.Duration.HasValue
                            ? Math.Clamp((int)(options.Duration.Value.TotalSeconds * targetSampleRate), 0, totalDecoded - sliceStart)
                            : totalDecoded - sliceStart;

                        for (int ch = 0; ch < targetChannels; ch++)
                        {
                            channelSamples[ch] = channelSamples[ch].GetRange(sliceStart, sliceCount);
                        }
                    }

                    var length = channelSamples.Count > 0 ? channelSamples[0].Count : 0;
                    var buffer = new AudioBuffer(targetChannels, targetSampleRate, length);
                    for (int ch = 0; ch < targetChannels; ch++)
                    {
                        if (channelSamples[ch].Count != length)
                        {
                            throw new InvalidOperationException("Channel sample count mismatch during decode.");
                        }

                        System.Runtime.InteropServices.CollectionsMarshal
                            .AsSpan(channelSamples[ch])
                            .CopyTo(buffer.GetChannelSpan(ch));
                    }

                    var durationSeconds = stream->duration > 0 && stream->time_base.den != 0
                        ? stream->duration * av_q2d(stream->time_base)
                        : (formatContext->duration != AV_NOPTS_VALUE
                            ? formatContext->duration / (double)AV_TIME_BASE
                            : 0);
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
        IList<List<float>> channelSamples,
        ResampleScratch scratch)
    {
        var dstNbSamples =
            ComputeResampleOutputSamples(resampler, sourceSampleRate, targetSampleRate, frame->nb_samples);
        var converted = scratch.Rent(targetChannels, dstNbSamples, targetFormat);

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

    private static unsafe void AppendSamples(
        AVFrame* frame,
        SwrContext* resampler,
        bool needsResample,
        int sourceSampleRate,
        int targetChannels,
        int targetSampleRate,
        AVSampleFormat targetFormat,
        IList<List<float>> channelSamples,
        ResampleScratch scratch)
    {
        if (needsResample)
        {
            ResampleInto(resampler, sourceSampleRate, frame, targetChannels, targetSampleRate, targetFormat,
                channelSamples, scratch);
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

    private sealed unsafe class ResampleScratch : IDisposable
    {
        private byte** _buffers;
        private int _channels;
        private int _capacity;
        private AVSampleFormat _format;

        public byte** Rent(int channels, int samples, AVSampleFormat format)
        {
            if (_buffers == null || _channels != channels || _capacity < samples || _format != format)
            {
                Release();
                _channels = channels;
                _capacity = samples;
                _format = format;

                byte** buffers = null;
                var ret = av_samples_alloc_array_and_samples(&buffers, null, channels, samples, format, 0);
                ThrowIfError(ret, nameof(av_samples_alloc_array_and_samples));
                _buffers = buffers;
            }

            return _buffers;
        }

        public void Dispose() => Release();

        private void Release()
        {
            if (_buffers == null)
            {
                return;
            }

            for (int ch = 0; ch < _channels; ch++)
            {
                if (_buffers[ch] != null)
                {
                    ffmpeg.av_free(_buffers[ch]);
                    _buffers[ch] = null;
                }
            }

            ffmpeg.av_free(_buffers);
            _buffers = null;
            _channels = 0;
            _capacity = 0;
            _format = AVSampleFormat.AV_SAMPLE_FMT_NONE;
        }
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