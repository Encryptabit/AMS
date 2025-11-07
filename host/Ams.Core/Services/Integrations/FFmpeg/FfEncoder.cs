using System;
using System.IO;
using System.Runtime.InteropServices;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;
using static Ams.Core.Services.Integrations.FFmpeg.FfUtils;

namespace Ams.Core.Services.Integrations.FFmpeg;

/// <summary>
/// FFmpeg-backed encoding helpers.
/// </summary>
internal static unsafe class FfEncoder
{
    private const int DefaultChunkSamples = 4096;
    private const int CustomIoBufferSize = 32 * 1024;

    public static void EncodeToDynamicBuffer(AudioBuffer buffer, Stream output, AudioEncodeOptions? options = null)
    {
        Encode(buffer, output, options ?? new AudioEncodeOptions(), EncoderSink.DynamicBuffer);
    }

    public static void EncodeToCustomStream(AudioBuffer buffer, Stream output, AudioEncodeOptions? options = null)
    {
        Encode(buffer, output, options ?? new AudioEncodeOptions(), EncoderSink.CustomStream);
    }

    private static void Encode(AudioBuffer buffer, Stream output, AudioEncodeOptions options, EncoderSink sink)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        ArgumentNullException.ThrowIfNull(output);

        FfSession.EnsureInitialized();

        var targetSampleRate = options.TargetSampleRate ?? buffer.SampleRate;
        var bitDepth = options.TargetBitDepth ?? 16;

        var (codecId, sampleFormat) = ResolveEncoding(bitDepth);

        AVCodec* codec = avcodec_find_encoder(codecId);
        if (codec == null)
        {
            throw new InvalidOperationException($"FFmpeg encoder for {codecId} not found.");
        }

        AVFormatContext* fmt = null;
        ThrowIfError(avformat_alloc_output_context2(&fmt, null, "wav", null), nameof(avformat_alloc_output_context2));
        if (fmt == null)
        {
            throw new InvalidOperationException("FFmpeg could not create a WAV format context.");
        }

        AVCodecContext* cc = null;
        AVStream* stream = null;
        AVIOContext* customIo = null;
        SwrContext* resampler = null;
        AVFrame* frame = null;
        GCHandle streamHandle = default;
        avio_alloc_context_write_packet? writeCallback = null;
        var pinnedChannels = Array.Empty<GCHandle>();
        var pinnedPointers = Array.Empty<IntPtr>();
        AVChannelLayout inputLayout = default;
        var inputLayoutInitialized = false;

        try
        {
            stream = avformat_new_stream(fmt, codec);
            if (stream == null)
            {
                throw new InvalidOperationException("avformat_new_stream failed");
            }

            cc = avcodec_alloc_context3(codec);
            if (cc == null)
            {
                throw new InvalidOperationException("avcodec_alloc_context3 failed");
            }

            cc->codec_id = codec->id;
            cc->codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;
            cc->sample_fmt = sampleFormat;
            cc->sample_rate = targetSampleRate;
            cc->time_base = new AVRational { num = 1, den = targetSampleRate };

            av_channel_layout_uninit(&cc->ch_layout);
            av_channel_layout_default(&cc->ch_layout, buffer.Channels);

            ThrowIfError(avcodec_open2(cc, codec, null), nameof(avcodec_open2));
            avcodec_parameters_from_context(stream->codecpar, cc);
            stream->time_base = cc->time_base;

            SetupIo(fmt, output, sink, ref customIo, ref streamHandle, ref writeCallback);
            ThrowIfError(avformat_write_header(fmt, null), nameof(avformat_write_header));

            inputLayout = CreateDefaultChannelLayout(buffer.Channels);
            inputLayoutInitialized = true;

            resampler = swr_alloc();
            if (resampler == null)
            {
                throw new InvalidOperationException("Failed to allocate FFmpeg resampler.");
            }

            var resamplerPtr = resampler;
            ThrowIfError(
                swr_alloc_set_opts2(
                    &resamplerPtr,
                    &cc->ch_layout,
                    cc->sample_fmt,
                    cc->sample_rate,
                    &inputLayout,
                    AVSampleFormat.AV_SAMPLE_FMT_FLTP,
                    buffer.SampleRate,
                    0,
                    null),
                nameof(swr_alloc_set_opts2));
            resampler = resamplerPtr;
            ThrowIfError(swr_init(resampler), nameof(swr_init));

            (pinnedChannels, pinnedPointers) = PinChannels(buffer);

            frame = AllocateFrame(cc);

            EncodeBuffer(buffer, cc, stream, fmt, resampler, frame, pinnedPointers, targetSampleRate);

            ThrowIfError(avcodec_send_frame(cc, null), nameof(avcodec_send_frame));
            DrainEncoder(cc, stream, fmt);

            ThrowIfError(av_write_trailer(fmt), nameof(av_write_trailer));

            FinalizeIo(fmt, output, sink, ref customIo);
        }
        finally
        {
            UnpinChannels(pinnedChannels);

            if (resampler != null)
            {
                swr_free(&resampler);
            }

            if (inputLayoutInitialized)
            {
                av_channel_layout_uninit(&inputLayout);
            }

            if (frame != null)
            {
                av_frame_free(&frame);
            }

            if (cc != null)
            {
                av_channel_layout_uninit(&cc->ch_layout);
                avcodec_free_context(&cc);
            }

            CleanupIo(fmt, sink, ref customIo, ref streamHandle, writeCallback);

            if (fmt != null)
            {
                avformat_free_context(fmt);
            }
        }
    }

    private static unsafe void EncodeBuffer(
        AudioBuffer buffer,
        AVCodecContext* cc,
        AVStream* stream,
        AVFormatContext* fmt,
        SwrContext* resampler,
        AVFrame* frame,
        IntPtr[] channelPointers,
        int targetSampleRate)
    {
        var totalSamples = buffer.Length;
        var channels = buffer.Channels;
        var cursor = 0;
        long pts = 0;

        while (cursor < totalSamples)
        {
            var chunk = Math.Min(DefaultChunkSamples, totalSamples - cursor);
            var dstCapacity = ComputeResampleOutputSamples(resampler, buffer.SampleRate, targetSampleRate, chunk);
            if (dstCapacity <= 0)
            {
                dstCapacity = chunk;
            }

            EnsureFrameCapacity(frame, cc, dstCapacity);
            ThrowIfError(av_frame_make_writable(frame), nameof(av_frame_make_writable));

            var src = stackalloc byte*[channels];
            for (int ch = 0; ch < channels; ch++)
            {
                var basePtr = (float*)channelPointers[ch];
                src[ch] = (byte*)(basePtr + cursor);
            }

            var produced = swr_convert(resampler, frame->extended_data, dstCapacity, src, chunk);
            ThrowIfError(produced, nameof(swr_convert));
            if (produced > 0)
            {
                frame->nb_samples = produced;
                frame->pts = pts;
                pts += produced;

                ThrowIfError(avcodec_send_frame(cc, frame), nameof(avcodec_send_frame));
                DrainEncoder(cc, stream, fmt);
            }

            cursor += chunk;
        }

        FlushResampler(buffer.SampleRate, targetSampleRate, resampler, frame, cc, stream, fmt, ref pts);
    }

    private static unsafe void FlushResampler(
        int sourceSampleRate,
        int targetSampleRate,
        SwrContext* resampler,
        AVFrame* frame,
        AVCodecContext* cc,
        AVStream* stream,
        AVFormatContext* fmt,
        ref long pts)
    {
        while (true)
        {
            var dstCapacity = ComputeResampleOutputSamples(resampler, sourceSampleRate, targetSampleRate, 0);
            if (dstCapacity <= 0)
            {
                if (cc->frame_size > 0)
                {
                    dstCapacity = cc->frame_size;
                }
                else
                {
                    break;
                }
            }

            EnsureFrameCapacity(frame, cc, dstCapacity);
            ThrowIfError(av_frame_make_writable(frame), nameof(av_frame_make_writable));

            var produced = swr_convert(resampler, frame->extended_data, dstCapacity, null, 0);
            ThrowIfError(produced, nameof(swr_convert));
            if (produced <= 0)
            {
                break;
            }

            frame->nb_samples = produced;
            frame->pts = pts;
            pts += produced;

            ThrowIfError(avcodec_send_frame(cc, frame), nameof(avcodec_send_frame));
            DrainEncoder(cc, stream, fmt);
        }
    }

    private static unsafe void DrainEncoder(AVCodecContext* cc, AVStream* stream, AVFormatContext* fmt)
    {
        while (true)
        {
            AVPacket* packet = av_packet_alloc();
            if (packet == null)
            {
                throw new InvalidOperationException("av_packet_alloc failed");
            }

            var receive = avcodec_receive_packet(cc, packet);
            if (receive == AVERROR(EAGAIN) || receive == AVERROR_EOF)
            {
                av_packet_free(&packet);
                break;
            }

            ThrowIfError(receive, nameof(avcodec_receive_packet));

            packet->stream_index = stream->index;
            av_packet_rescale_ts(packet, cc->time_base, stream->time_base);
            ThrowIfError(av_write_frame(fmt, packet), nameof(av_write_frame));

            av_packet_free(&packet);
        }
    }

    private static unsafe void EnsureFrameCapacity(AVFrame* frame, AVCodecContext* cc, int requiredSamples)
    {
        if (requiredSamples <= 0)
        {
            requiredSamples = cc->frame_size > 0 ? cc->frame_size : DefaultChunkSamples;
        }

        if (frame->nb_samples == requiredSamples && frame->data[0] != null)
        {
            return;
        }

        av_frame_unref(frame);
        frame->format = (int)cc->sample_fmt;
        frame->sample_rate = cc->sample_rate;
        ThrowIfError(av_channel_layout_copy(&frame->ch_layout, &cc->ch_layout), nameof(av_channel_layout_copy));
        frame->nb_samples = requiredSamples;
        ThrowIfError(av_frame_get_buffer(frame, 0), nameof(av_frame_get_buffer));
    }

    private static unsafe AVFrame* AllocateFrame(AVCodecContext* cc)
    {
        AVFrame* frame = av_frame_alloc();
        if (frame == null)
        {
            throw new InvalidOperationException("av_frame_alloc failed");
        }

        frame->format = (int)cc->sample_fmt;
        frame->sample_rate = cc->sample_rate;
        ThrowIfError(av_channel_layout_copy(&frame->ch_layout, &cc->ch_layout), nameof(av_channel_layout_copy));
        frame->nb_samples = 0;
        return frame;
    }

    private static (GCHandle[] Handles, IntPtr[] Pointers) PinChannels(AudioBuffer buffer)
    {
        var handles = new GCHandle[buffer.Channels];
        var pointers = new IntPtr[buffer.Channels];

        for (int ch = 0; ch < buffer.Channels; ch++)
        {
            var channel = buffer.Planar[ch];
            if (channel == null)
            {
                throw new InvalidOperationException($"Audio channel {ch} is null.");
            }

            handles[ch] = GCHandle.Alloc(channel, GCHandleType.Pinned);
            pointers[ch] = handles[ch].AddrOfPinnedObject();
        }

        return (handles, pointers);
    }

    private static void UnpinChannels(GCHandle[] handles)
    {
        if (handles.Length == 0)
        {
            return;
        }

        foreach (var handle in handles)
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }

    private static (AVCodecID CodecId, AVSampleFormat SampleFormat) ResolveEncoding(int bitDepth)
    {
        return bitDepth switch
        {
            16 => (AVCodecID.AV_CODEC_ID_PCM_S16LE, AVSampleFormat.AV_SAMPLE_FMT_S16),
            32 => (AVCodecID.AV_CODEC_ID_PCM_F32LE, AVSampleFormat.AV_SAMPLE_FMT_FLT),
            _ => throw new NotSupportedException($"Unsupported PCM bit depth {bitDepth}."),
        };
    }

    private static unsafe void SetupIo(
        AVFormatContext* fmt,
        Stream output,
        EncoderSink sink,
        ref AVIOContext* customIo,
        ref GCHandle handle,
        ref avio_alloc_context_write_packet? writeCallback)
    {
        if (sink == EncoderSink.DynamicBuffer)
        {
            ThrowIfError(avio_open_dyn_buf(&fmt->pb), nameof(avio_open_dyn_buf));
            return;
        }

        var ioBuffer = (byte*)av_malloc((nuint)CustomIoBufferSize);
        if (ioBuffer == null)
        {
            throw new OutOfMemoryException("av_malloc failed for AVIO buffer");
        }

        handle = GCHandle.Alloc(output, GCHandleType.Normal);
        writeCallback = AvioWritePacket;

        var handlePtr = GCHandle.ToIntPtr(handle);

        customIo = avio_alloc_context(
            ioBuffer,
            CustomIoBufferSize,
            1,
            (void*)handlePtr,
            null,
            writeCallback,
            null);

        if (customIo == null)
        {
            av_free(ioBuffer);
            handle.Free();
            throw new InvalidOperationException("avio_alloc_context failed");
        }

        fmt->pb = customIo;
        fmt->flags |= AVFMT_FLAG_CUSTOM_IO;
    }

    private static unsafe void FinalizeIo(AVFormatContext* fmt, Stream output, EncoderSink sink,
        ref AVIOContext* customIo)
    {
        if (sink == EncoderSink.DynamicBuffer)
        {
            byte* raw = null;
            var size = avio_close_dyn_buf(fmt->pb, &raw);
            ThrowIfError(size, nameof(avio_close_dyn_buf));

            try
            {
                var managed = new byte[size];
                Marshal.Copy((IntPtr)raw, managed, 0, size);
                output.Write(managed, 0, managed.Length);
            }
            finally
            {
                av_free(raw);
            }

            fmt->pb = null;
            return;
        }

        if (customIo != null)
        {
            avio_flush(customIo);
        }
    }

    private static unsafe void CleanupIo(
        AVFormatContext* fmt,
        EncoderSink sink,
        ref AVIOContext* customIo,
        ref GCHandle handle,
        avio_alloc_context_write_packet? writeCallback)
    {
        if (sink == EncoderSink.CustomStream)
        {
            if (customIo != null)
            {
                if (customIo->buffer != null)
                {
                    av_free(customIo->buffer);
                    customIo->buffer = null;
                }

                var localIo = customIo;
                avio_context_free(&localIo);
                customIo = null;
            }

            if (handle.IsAllocated)
            {
                handle.Free();
            }

            if (writeCallback != null)
            {
                GC.KeepAlive(writeCallback);
            }
        }

        if (sink == EncoderSink.DynamicBuffer && fmt != null && fmt->pb != null)
        {
            byte* raw = null;
            avio_close_dyn_buf(fmt->pb, &raw);
            if (raw != null)
            {
                av_free(raw);
            }

            fmt->pb = null;
        }
    }

    private static unsafe int AvioWritePacket(void* opaque, byte* buf, int bufSize)
    {
        try
        {
            var handle = GCHandle.FromIntPtr((IntPtr)opaque);
            var stream = (Stream)handle.Target!;
            stream.Write(new ReadOnlySpan<byte>(buf, bufSize));
            return bufSize;
        }
        catch
        {
            return AVERROR_EOF;
        }
    }

    private enum EncoderSink
    {
        DynamicBuffer,
        CustomStream,
    }
}
