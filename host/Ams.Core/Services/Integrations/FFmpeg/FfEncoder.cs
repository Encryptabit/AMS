using System.Runtime.InteropServices;
using Ams.Core.Artifacts;
using Ams.Core.Processors;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg;

/// <summary>
/// Placeholder for FFmpeg-backed encoding helpers.
/// </summary>
internal static unsafe class FfEncoder
{
     public static void AudioBufferToWavStream(AudioBuffer buf, Stream output, PcmEncoding encoding = PcmEncoding.Pcm32)
    {
        // 1) Find encoder (PCM F32LE or PCM F16LE) and muxer (WAV)
        ffmpeg.av_get_pcm_codec(AVSampleFormat.AV_SAMPLE_FMT_FLT, 0);
        AVCodec* codec = ffmpeg.avcodec_find_encoder(ffmpeg.av_get_pcm_codec(AVSampleFormat.AV_SAMPLE_FMT_FLT, 0));
        if (codec == null) throw new InvalidOperationException("PCM_F32LE encoder not found");

        AVFormatContext* fmt = null;
        ffmpeg.avformat_alloc_output_context2(&fmt, null, "wav", null);
        if (fmt == null) throw new InvalidOperationException("WAV muxer not available");

        // 2) New stream + codec context
        AVStream* st = ffmpeg.avformat_new_stream(fmt, codec);
        if (st == null) throw new InvalidOperationException("avformat_new_stream failed");

        AVCodecContext* cc = ffmpeg.avcodec_alloc_context3(codec);
        if (cc == null) throw new InvalidOperationException("avcodec_alloc_context3 failed");

        cc->codec_id = codec->id;
        cc->codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;
        cc->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_FLT; // float32 PCM
        cc->sample_rate = buf.SampleRate;
        cc->time_base = new AVRational { num = 1, den = buf.SampleRate };

        vectors.av_channel_layout_uninit(&cc->ch_layout);
        vectors.av_channel_layout_default(&cc->ch_layout, buf.Channels);

        int ret;
        if ((ret = ffmpeg.avcodec_open2(cc, codec, null)) < 0) throw new ApplicationException($"avcodec_open2: {ret}");

        // Copy codec params to stream
        ret = ffmpeg.avcodec_parameters_from_context(st->codecpar, cc);
        if (ret < 0) throw new ApplicationException($"avcodec_parameters_from_context: {ret}");
        st->time_base = cc->time_base;

        // 3) Use dynamic avio buffer
        byte* dynBuf = null;
        AVIOContext* pb = null;
        pb = ffmpeg.avio_alloc_context(null, 0, 1, null, null, null, null); // dummy; will be replaced by open_dyn_buf
        if (pb == null) throw new ApplicationException("avio_alloc_context failed");
        ret = ffmpeg.avio_open_dyn_buf(&fmt->pb);
        if (ret < 0) throw new ApplicationException($"avio_open_dyn_buf: {ret}");

        // 4) Write header
        ret = ffmpeg.avformat_write_header(fmt, null);
        if (ret < 0) throw new ApplicationException($"avformat_write_header: {ret}");

        // 5) Encode & write packets
        // We’ll feed frames of N samples until we exhaust buffer length.
        const int frameSamples = 1024; // any reasonable block size
        int totalSamples = buf.Length; // samples per channel
        long pts = 0;

        // Allocate one AVFrame
        AVFrame* frame = ffmpeg.av_frame_alloc();
        frame->nb_samples = frameSamples;
        frame->format = (int)cc->sample_fmt;
        frame->sample_rate = cc->sample_rate;

        ffmpeg.av_channel_layout_copy(&frame->ch_layout, &cc->ch_layout);
        
        if ((ret = ffmpeg.av_frame_get_buffer(frame, 0)) < 0) throw new ApplicationException($"av_frame_get_buffer: {ret}");

        int cursor = 0;
        while (cursor < totalSamples)
        {
            int n = Math.Min(frameSamples, totalSamples - cursor);

            // Make sure frame has writable buffers sized for n samples
            if (frame->nb_samples != n)
            {
                frame->nb_samples = n;
                ffmpeg.av_frame_unref(frame);
                if ((ret = ffmpeg.av_frame_get_buffer(frame, 0)) < 0) throw new ApplicationException($"av_frame_get_buffer: {ret}");
            }

            // Write planar floats into frame->extended_data[c]
            for (int ch = 0; ch < buf.Channels; ch++)
            {
                float* dst = (float*)frame->extended_data[ch];
                var src = buf.Planar[ch];
                for (int i = 0; i < n; i++)
                    dst[i] = src[cursor + i];
            }

            frame->pts = pts;
            pts += n;

            // Send & receive packets
            if ((ret = ffmpeg.avcodec_send_frame(cc, frame)) < 0) throw new ApplicationException($"avcodec_send_frame: {ret}");

            while (true)
            {
                AVPacket* pkt = ffmpeg.av_packet_alloc();
                ret = ffmpeg.avcodec_receive_packet(cc, pkt);
                if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                {
                    ffmpeg.av_packet_free(&pkt);
                    break;
                }

                if (ret < 0)
                {
                    ffmpeg.av_packet_free(&pkt);
                    throw new ApplicationException($"receive_packet: {ret}");
                }

                pkt->stream_index = st->index;
                // rescale PTS/DTS if needed
                ffmpeg.av_packet_rescale_ts(pkt, cc->time_base, st->time_base);

                if ((ret = ffmpeg.av_write_frame(fmt, pkt)) < 0)
                {
                    ffmpeg.av_packet_free(&pkt);
                    throw new ApplicationException($"av_write_frame: {ret}");
                }

                ffmpeg.av_packet_free(&pkt);
            }

            cursor += n;
        }

        // Flush encoder
        if ((ret = ffmpeg.avcodec_send_frame(cc, null)) == 0)
        {
            // Send & receive packets
            while (true)
            {
                AVPacket* pkt = ffmpeg.av_packet_alloc();
                ret = ffmpeg.avcodec_receive_packet(cc, pkt);
                if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                {
                    ffmpeg.av_packet_free(&pkt);
                    break;
                }

                if (ret < 0)
                {
                    ffmpeg.av_packet_free(&pkt);
                    throw new ApplicationException($"flush receive_packet: {ret}");
                }

                pkt->stream_index = st->index;
                ffmpeg.av_packet_rescale_ts(pkt, cc->time_base, st->time_base);
                if ((ret = ffmpeg.av_write_frame(fmt, pkt)) < 0)
                {
                    ffmpeg.av_packet_free(&pkt);
                    throw new ApplicationException($"av_write_frame (flush): {ret}");
                }

                ffmpeg.av_packet_free(&pkt);
            }
        }

        // Trailer
        if ((ret = ffmpeg.av_write_trailer(fmt)) < 0) throw new ApplicationException($"av_write_trailer: {ret}");

        // 6) Grab FFmpeg's dyn buffer and copy into Stream
        int dynSize = ffmpeg.avio_close_dyn_buf(fmt->pb, &dynBuf);
        if (dynSize < 0) throw new ApplicationException($"avio_close_dyn_buf: {dynSize}");

        // Copy to managed Stream
        byte[] managed = new byte[dynSize];
        Marshal.Copy((IntPtr)dynBuf, managed, 0, dynSize);
        output.Write(managed, 0, managed.Length);

        // Free FFmpeg buffer
        ffmpeg.av_free(dynBuf);

        // Cleanup
        ffmpeg.av_frame_free(&frame);
        ffmpeg.avcodec_free_context(&cc);
        ffmpeg.avformat_free_context(fmt);
        vectors.av_channel_layout_uninit(&frame->ch_layout);
    }
}


public enum PcmEncoding
{
    Pcm16,
    Pcm32,
}