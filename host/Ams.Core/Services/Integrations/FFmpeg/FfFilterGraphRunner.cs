using System;
using System.Collections.Generic;
using Ams.Core.Artifacts;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg;

internal static class FfFilterGraphRunner
{
    internal enum FilterExecutionMode
    {
        ReturnAudio,
        DiscardOutput,
    }

    public static AudioBuffer Apply(AudioBuffer input, string filterSpec)
    {
        return ExecuteInternal(input, filterSpec, FilterExecutionMode.ReturnAudio) ?? throw new InvalidOperationException("FFmpeg filter graph did not produce audio output.");
    }

    public static void Execute(AudioBuffer input, string filterSpec, FilterExecutionMode mode)
    {
        ExecuteInternal(input, filterSpec, mode);
    }

    private static AudioBuffer? ExecuteInternal(AudioBuffer input, string filterSpec, FilterExecutionMode mode)
    {
        using var executor = new FilterGraphExecutor(input, filterSpec, mode);
        executor.Process();
        return executor.BuildOutput();
    }

    private unsafe sealed class FilterGraphExecutor : IDisposable
    {
        private const int ChunkSamples = 4096;

        private readonly AudioBuffer _input;
        private readonly string _filterSpec;
        private readonly FilterExecutionMode _mode;

        private AVFilterGraph* _graph;
        private AVFilterContext* _src;
        private AVFilterContext* _sink;
        private AVFrame* _inputFrame;
        private AVFrame* _outputFrame;
        private AudioAccumulator? _accumulator;
        private readonly int _channels;
        private readonly int _sampleRate;

        public FilterGraphExecutor(AudioBuffer input, string filterSpec, FilterExecutionMode mode)
        {
            FfSession.EnsureFiltersAvailable();
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _filterSpec = string.IsNullOrWhiteSpace(filterSpec) ? "anull" : filterSpec;
            _mode = mode;
            _graph = ffmpeg.avfilter_graph_alloc();
            if (_graph == null)
            {
                throw new InvalidOperationException("Failed to allocate FFmpeg filter graph.");
            }

            _channels = input.Channels;
            _sampleRate = input.SampleRate;

            SetupSource();
            SetupSink();
            ConfigureGraph();

            _inputFrame = ffmpeg.av_frame_alloc();
            if (_inputFrame == null)
            {
                throw new InvalidOperationException("Failed to allocate FFmpeg frame.");
            }

            _outputFrame = ffmpeg.av_frame_alloc();
            if (_outputFrame == null)
            {
                throw new InvalidOperationException("Failed to allocate FFmpeg frame.");
            }

            if (_mode == FilterExecutionMode.ReturnAudio)
            {
                _accumulator = new AudioAccumulator(_channels, _sampleRate);
            }
        }

        public void Process()
        {
            int totalSamples = _input.Length;
            int offset = 0;
            long pts = 0;

            while (offset < totalSamples)
            {
                int chunk = Math.Min(ChunkSamples, totalSamples - offset);
                SendFrame(offset, chunk, pts);
                pts += chunk;
                Drain();
                offset += chunk;
            }

            FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame(_src, null), nameof(ffmpeg.av_buffersrc_add_frame));
            Drain(final: true);
        }

        public AudioBuffer? BuildOutput()
        {
            return _accumulator?.ToBuffer();
        }

        private void SendFrame(int offset, int sampleCount, long pts)
        {
            ffmpeg.av_frame_unref(_inputFrame);
            _inputFrame->nb_samples = sampleCount;
            _inputFrame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_FLT;
            _inputFrame->sample_rate = _sampleRate;
            ffmpeg.av_channel_layout_uninit(&_inputFrame->ch_layout);
            ffmpeg.av_channel_layout_default(&_inputFrame->ch_layout, _channels);
            FfUtils.ThrowIfError(ffmpeg.av_frame_get_buffer(_inputFrame, 0), nameof(ffmpeg.av_frame_get_buffer));
            _inputFrame->pts = pts;

            var planar = _input.Planar;
            float* destination = (float*)_inputFrame->data[0];
            for (int sample = 0; sample < sampleCount; sample++)
            {
                int baseIndex = sample * _channels;
                for (int ch = 0; ch < _channels; ch++)
                {
                    destination[baseIndex + ch] = planar[ch][offset + sample];
                }
            }

            FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame(_src, _inputFrame), nameof(ffmpeg.av_buffersrc_add_frame));
        }

        private void Drain(bool final = false)
        {
            while (true)
            {
                int ret = ffmpeg.av_buffersink_get_frame(_sink, _outputFrame);
                if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                {
                    break;
                }

                if (ret == ffmpeg.AVERROR_EOF)
                {
                    break;
                }

                FfUtils.ThrowIfError(ret, nameof(ffmpeg.av_buffersink_get_frame));

                if (_accumulator is not null)
                {
                    _accumulator.Add(_outputFrame);
                }

                ffmpeg.av_frame_unref(_outputFrame);
            }

            if (final)
            {
                ffmpeg.av_frame_unref(_outputFrame);
            }
        }

        private void SetupSource()
        {
            var abuffer = ffmpeg.avfilter_get_by_name("abuffer");
            if (abuffer == null)
            {
                throw new InvalidOperationException("FFmpeg 'abuffer' filter not found.");
            }

            _src = ffmpeg.avfilter_graph_alloc_filter(_graph, abuffer, "src");
            if (_src == null)
            {
                throw new InvalidOperationException("Failed to allocate audio buffer source filter.");
            }

            var timeBase = new AVRational { num = 1, den = _sampleRate };
            FfUtils.ThrowIfError(ffmpeg.av_opt_set_q(_src, "time_base", timeBase, 0), nameof(ffmpeg.av_opt_set_q));
            FfUtils.ThrowIfError(ffmpeg.av_opt_set_int(_src, "sample_rate", _sampleRate, 0), nameof(ffmpeg.av_opt_set_int));

            var sampleFmtName = ffmpeg.av_get_sample_fmt_name(AVSampleFormat.AV_SAMPLE_FMT_FLT);
            if (sampleFmtName == null)
            {
                throw new InvalidOperationException("Failed to resolve sample format name.");
            }

            FfUtils.ThrowIfError(ffmpeg.av_opt_set(_src, "sample_fmt", sampleFmtName, 0), nameof(ffmpeg.av_opt_set));

            AVChannelLayout* srcLayoutPtr = stackalloc AVChannelLayout[1];
            ffmpeg.av_channel_layout_default(srcLayoutPtr, _channels);
            try
            {
                FfUtils.ThrowIfError(ffmpeg.av_opt_set_chlayout(_src, "ch_layout", srcLayoutPtr, 0), nameof(ffmpeg.av_opt_set_chlayout));
            }
            finally
            {
                ffmpeg.av_channel_layout_uninit(srcLayoutPtr);
            }

            FfUtils.ThrowIfError(ffmpeg.avfilter_init_str(_src, null), nameof(ffmpeg.avfilter_init_str));
        }

        private void SetupSink()
        {
            var abuffersink = ffmpeg.avfilter_get_by_name("abuffersink");
            if (abuffersink == null)
            {
                throw new InvalidOperationException("FFmpeg 'abuffersink' filter not found.");
            }

            _sink = ffmpeg.avfilter_graph_alloc_filter(_graph, abuffersink, "sink");
            if (_sink == null)
            {
                throw new InvalidOperationException("Failed to allocate audio buffer sink filter.");
            }

            FfUtils.ThrowIfError(ffmpeg.avfilter_init_str(_sink, null), nameof(ffmpeg.avfilter_init_str));

            AVChannelLayout* sinkLayoutPtr = stackalloc AVChannelLayout[1];
            ffmpeg.av_channel_layout_default(sinkLayoutPtr, _channels);
            try
            {
                FfUtils.ThrowIfError(ffmpeg.av_opt_set_chlayout(_sink, "ch_layout", sinkLayoutPtr, 0), nameof(ffmpeg.av_opt_set_chlayout));
            }
            finally
            {
                ffmpeg.av_channel_layout_uninit(sinkLayoutPtr);
            }

            FfUtils.ThrowIfError(ffmpeg.av_opt_set_sample_fmt(_sink, "sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_FLT, 0), nameof(ffmpeg.av_opt_set_sample_fmt));
            FfUtils.ThrowIfError(ffmpeg.av_opt_set_int(_sink, "sample_rate", _sampleRate, 0), nameof(ffmpeg.av_opt_set_int));
        }

        private void ConfigureGraph()
        {
            AVFilterInOut* outputs = ffmpeg.avfilter_inout_alloc();
            AVFilterInOut* inputs = ffmpeg.avfilter_inout_alloc();

            try
            {
                if (outputs == null || inputs == null)
                {
                    throw new InvalidOperationException("Failed to allocate FFmpeg filter in/out structures.");
                }

                outputs->name = ffmpeg.av_strdup("in");
                outputs->filter_ctx = _src;
                outputs->pad_idx = 0;
                outputs->next = null;

                inputs->name = ffmpeg.av_strdup("out");
                inputs->filter_ctx = _sink;
                inputs->pad_idx = 0;
                inputs->next = null;

                FfUtils.ThrowIfError(ffmpeg.avfilter_graph_parse_ptr(_graph, _filterSpec, &inputs, &outputs, null), nameof(ffmpeg.avfilter_graph_parse_ptr));
                FfUtils.ThrowIfError(ffmpeg.avfilter_graph_config(_graph, null), nameof(ffmpeg.avfilter_graph_config));
            }
            finally
            {
                if (outputs != null)
                {
                    ffmpeg.avfilter_inout_free(&outputs);
                }

                if (inputs != null)
                {
                    ffmpeg.avfilter_inout_free(&inputs);
                }
            }
        }

        public void Dispose()
        {
            if (_outputFrame != null)
            {
                var frame = _outputFrame;
                ffmpeg.av_frame_free(&frame);
                _outputFrame = frame;
            }

            if (_inputFrame != null)
            {
                var frame = _inputFrame;
                ffmpeg.av_frame_free(&frame);
                _inputFrame = frame;
            }

            if (_graph != null)
            {
                var graph = _graph;
                ffmpeg.avfilter_graph_free(&graph);
                _graph = graph;
            }
        }
    }

    private sealed class AudioAccumulator
    {
        private readonly List<float>[] _channels;
        private int _sampleRate;
        private bool _sampleRateSet;

        public AudioAccumulator(int channelCount, int sampleRate)
        {
            _channels = new List<float>[channelCount];
            for (int i = 0; i < channelCount; i++)
            {
                _channels[i] = new List<float>(8192);
            }

            _sampleRate = sampleRate;
            _sampleRateSet = sampleRate > 0;
        }

        public unsafe void Add(AVFrame* frame)
        {
            int channels = frame->ch_layout.nb_channels;
            int samples = frame->nb_samples;

            if (!_sampleRateSet && frame->sample_rate > 0)
            {
                _sampleRate = frame->sample_rate;
                _sampleRateSet = true;
            }

            float* data = (float*)frame->data[0];
            for (int sample = 0; sample < samples; sample++)
            {
                int baseIndex = sample * channels;
                for (int ch = 0; ch < channels; ch++)
                {
                    _channels[ch].Add(data[baseIndex + ch]);
                }
            }
        }

        public AudioBuffer ToBuffer()
        {
            if (!_sampleRateSet)
            {
                throw new InvalidOperationException("FFmpeg filter graph did not report an output sample rate.");
            }

            int length = _channels.Length > 0 ? _channels[0].Count : 0;
            var buffer = new AudioBuffer(_channels.Length, _sampleRate, length);
            for (int ch = 0; ch < _channels.Length; ch++)
            {
                _channels[ch].CopyTo(buffer.Planar[ch], 0);
            }

            return buffer;
        }
    }
}
