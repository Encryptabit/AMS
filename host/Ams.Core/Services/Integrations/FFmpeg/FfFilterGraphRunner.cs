using System;
using System.Collections.Generic;
using Ams.Core.Artifacts;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg;

internal static class FfFilterGraphRunner
{
    internal readonly record struct GraphInput(string Label, AudioBuffer Buffer);

    internal enum FilterExecutionMode
    {
        ReturnAudio,
        DiscardOutput,
    }

    public static AudioBuffer Apply(AudioBuffer input, string filterSpec)
    {
        var inputs = new[] { new GraphInput("main", input) };
        return ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio) ?? throw new InvalidOperationException("FFmpeg filter graph did not produce audio output.");
    }

    public static AudioBuffer Apply(IReadOnlyList<GraphInput> inputs, string filterSpec)
    {
        if (inputs is null || inputs.Count == 0)
        {
            throw new ArgumentException("At least one input is required.", nameof(inputs));
        }

        return ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio) ?? throw new InvalidOperationException("FFmpeg filter graph did not produce audio output.");
    }

    public static void Execute(AudioBuffer input, string filterSpec, FilterExecutionMode mode)
    {
        var inputs = new[] { new GraphInput("main", input) };
        ExecuteInternal(inputs, filterSpec, mode);
    }

    public static void Execute(IReadOnlyList<GraphInput> inputs, string filterSpec, FilterExecutionMode mode)
    {
        if (inputs is null || inputs.Count == 0)
        {
            throw new ArgumentException("At least one input is required.", nameof(inputs));
        }

        ExecuteInternal(inputs, filterSpec, mode);
    }

    private static AudioBuffer? ExecuteInternal(IReadOnlyList<GraphInput> inputs, string filterSpec, FilterExecutionMode mode)
    {
        using var executor = new FilterGraphExecutor(inputs, filterSpec, mode);
        executor.Process();
        return executor.BuildOutput();
    }

    private unsafe sealed class FilterGraphExecutor : IDisposable
    {
        private const int ChunkSamples = 4096;

        private readonly GraphInputState[] _inputs;
        private readonly string _filterSpec;
        private readonly FilterExecutionMode _mode;

        private AVFilterGraph* _graph;
        private AVFilterContext* _sink;
        private AVFrame* _outputFrame;
        private AudioAccumulator? _accumulator;
        private readonly int _channels;
        private readonly int _sampleRate;

        public FilterGraphExecutor(IReadOnlyList<GraphInput> inputs, string filterSpec, FilterExecutionMode mode)
        {
            FfSession.EnsureFiltersAvailable();
            _filterSpec = string.IsNullOrWhiteSpace(filterSpec) ? "anull" : filterSpec;
            _mode = mode;
            _graph = ffmpeg.avfilter_graph_alloc();
            if (_graph == null)
            {
                throw new InvalidOperationException("Failed to allocate FFmpeg filter graph.");
            }

            _inputs = CreateInputs(inputs);
            if (_inputs.Length == 0)
            {
                throw new ArgumentException("At least one input is required.");
            }

            _channels = _inputs[0].Channels;
            _sampleRate = _inputs[0].SampleRate;

            SetupSink();
            ConfigureGraph();

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
            foreach (var input in _inputs)
            {
                SendAllFrames(input);
                FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame(input.Source, null), nameof(ffmpeg.av_buffersrc_add_frame));
            }

            Drain(final: true);
        }

        public AudioBuffer? BuildOutput()
        {
            return _accumulator?.ToBuffer();
        }

        private void SendAllFrames(GraphInputState state)
        {
            int totalSamples = state.Buffer.Length;
            int offset = 0;
            long pts = 0;

            while (offset < totalSamples)
            {
                int chunk = Math.Min(ChunkSamples, totalSamples - offset);
                SendFrame(state, offset, chunk, pts);
                pts += chunk;
                offset += chunk;
                Drain();
            }
        }

        private void SendFrame(GraphInputState state, int offset, int sampleCount, long pts)
        {
            ffmpeg.av_frame_unref(state.Frame);
            state.Frame->nb_samples = sampleCount;
            state.Frame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_FLT;
            state.Frame->sample_rate = state.SampleRate;
            ffmpeg.av_channel_layout_uninit(&state.Frame->ch_layout);
            ffmpeg.av_channel_layout_default(&state.Frame->ch_layout, state.Channels);
            FfUtils.ThrowIfError(ffmpeg.av_frame_get_buffer(state.Frame, 0), nameof(ffmpeg.av_frame_get_buffer));
            state.Frame->pts = pts;

            var planar = state.Buffer.Planar;
            float* destination = (float*)state.Frame->data[0];
            for (int sample = 0; sample < sampleCount; sample++)
            {
                int baseIndex = sample * state.Channels;
                for (int ch = 0; ch < state.Channels; ch++)
                {
                    destination[baseIndex + ch] = planar[ch][offset + sample];
                }
            }

            FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame(state.Source, state.Frame), nameof(ffmpeg.av_buffersrc_add_frame));
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

        private GraphInputState[] CreateInputs(IReadOnlyList<GraphInput> inputs)
        {
            var result = new GraphInputState[inputs.Count];
            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                if (input.Buffer is null)
                {
                    throw new ArgumentNullException(nameof(inputs), "Input buffer cannot be null.");
                }

                string label = string.IsNullOrWhiteSpace(input.Label) ? $"in{i}" : input.Label;
                result[i] = SetupSource(label, input.Buffer);
            }

            return result;
        }

        private GraphInputState SetupSource(string label, AudioBuffer buffer)
        {
            var abuffer = ffmpeg.avfilter_get_by_name("abuffer");
            if (abuffer == null)
            {
                throw new InvalidOperationException("FFmpeg 'abuffer' filter not found.");
            }

            var src = ffmpeg.avfilter_graph_alloc_filter(_graph, abuffer, label);
            if (src == null)
            {
                throw new InvalidOperationException("Failed to allocate audio buffer source filter.");
            }

            var timeBase = new AVRational { num = 1, den = buffer.SampleRate };
            FfUtils.ThrowIfError(ffmpeg.av_opt_set_q(src, "time_base", timeBase, 0), nameof(ffmpeg.av_opt_set_q));
            FfUtils.ThrowIfError(ffmpeg.av_opt_set_int(src, "sample_rate", buffer.SampleRate, 0), nameof(ffmpeg.av_opt_set_int));

            var sampleFmtName = ffmpeg.av_get_sample_fmt_name(AVSampleFormat.AV_SAMPLE_FMT_FLT);
            if (sampleFmtName == null)
            {
                throw new InvalidOperationException("Failed to resolve sample format name.");
            }

            FfUtils.ThrowIfError(ffmpeg.av_opt_set(src, "sample_fmt", sampleFmtName, 0), nameof(ffmpeg.av_opt_set));

            AVChannelLayout* srcLayoutPtr = stackalloc AVChannelLayout[1];
            ffmpeg.av_channel_layout_default(srcLayoutPtr, buffer.Channels);
            try
            {
                FfUtils.ThrowIfError(ffmpeg.av_opt_set_chlayout(src, "ch_layout", srcLayoutPtr, 0), nameof(ffmpeg.av_opt_set_chlayout));
            }
            finally
            {
                ffmpeg.av_channel_layout_uninit(srcLayoutPtr);
            }

            FfUtils.ThrowIfError(ffmpeg.avfilter_init_str(src, null), nameof(ffmpeg.avfilter_init_str));

            var frame = ffmpeg.av_frame_alloc();
            if (frame == null)
            {
                throw new InvalidOperationException("Failed to allocate FFmpeg frame.");
            }

            return new GraphInputState(label, buffer, src, frame);
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
            AVFilterInOut* outputs = null;
            AVFilterInOut* inputs = ffmpeg.avfilter_inout_alloc();

            try
            {
                if (inputs == null)
                {
                    throw new InvalidOperationException("Failed to allocate FFmpeg filter in/out structures.");
                }

                for (int i = _inputs.Length - 1; i >= 0; i--)
                {
                    var inout = ffmpeg.avfilter_inout_alloc();
                    if (inout == null)
                    {
                        throw new InvalidOperationException("Failed to allocate FFmpeg filter in/out structures.");
                    }

                    inout->name = ffmpeg.av_strdup(_inputs[i].Label);
                    inout->filter_ctx = _inputs[i].Source;
                    inout->pad_idx = 0;
                    inout->next = outputs;
                    outputs = inout;
                }

                inputs->name = ffmpeg.av_strdup("out");
                inputs->filter_ctx = _sink;
                inputs->pad_idx = 0;
                inputs->next = null;

                FfUtils.ThrowIfError(ffmpeg.avfilter_graph_parse_ptr(_graph, _filterSpec, &inputs, &outputs, null), nameof(ffmpeg.avfilter_graph_parse_ptr));
                FfUtils.ThrowIfError(ffmpeg.avfilter_graph_config(_graph, null), nameof(ffmpeg.avfilter_graph_config));
            }
            finally
            {
                while (outputs != null)
                {
                    var next = outputs->next;
                    ffmpeg.avfilter_inout_free(&outputs);
                    outputs = next;
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

            if (_graph != null)
            {
                var graph = _graph;
                ffmpeg.avfilter_graph_free(&graph);
                _graph = graph;
            }

            if (_inputs != null)
            {
                foreach (var input in _inputs)
                {
                    if (input.Frame != null)
                    {
                        var frame = input.Frame;
                        ffmpeg.av_frame_free(&frame);
                    }
                }
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

    private unsafe sealed class GraphInputState
    {
        public GraphInputState(string label, AudioBuffer buffer, AVFilterContext* source, AVFrame* frame)
        {
            Label = label;
            Buffer = buffer;
            Source = source;
            Frame = frame;
            SampleRate = buffer.SampleRate;
            Channels = buffer.Channels;
        }

        public string Label { get; }
        public AudioBuffer Buffer { get; }
        public AVFilterContext* Source { get; }
        public AVFrame* Frame { get; }
        public int SampleRate { get; }
        public int Channels { get; }
    }
}
