
using System;
using System.Collections.Generic;
using Ams.Core.Artifacts;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg
{
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
            return ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio)
                   ?? throw new InvalidOperationException("FFmpeg filter graph did not produce audio output.");
        }

        public static AudioBuffer Apply(IReadOnlyList<GraphInput> inputs, string filterSpec)
        {
            if (inputs is null || inputs.Count == 0)
                throw new ArgumentException("At least one input is required.", nameof(inputs));

            return ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio)
                   ?? throw new InvalidOperationException("FFmpeg filter graph did not produce audio output.");
        }

        public static void Execute(AudioBuffer input, string filterSpec, FilterExecutionMode mode)
        {
            var inputs = new[] { new GraphInput("main", input) };
            ExecuteInternal(inputs, filterSpec, mode);
        }

        public static void Execute(IReadOnlyList<GraphInput> inputs, string filterSpec, FilterExecutionMode mode)
        {
            if (inputs is null || inputs.Count == 0)
                throw new ArgumentException("At least one input is required.", nameof(inputs));

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
            // Tuneable: larger chunk = fewer calls, smaller chunk = lower latency.
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

                // Let parse/connect wire sources->spec->sink. If empty, use a no-op.
                _filterSpec = string.IsNullOrWhiteSpace(filterSpec) ? "anull" : filterSpec;
                _mode = mode;

                _graph = ffmpeg.avfilter_graph_alloc();
                if (_graph == null)
                    throw new InvalidOperationException("Failed to allocate FFmpeg filter graph.");

                _inputs = CreateInputs(inputs);
                if (_inputs.Length == 0)
                    throw new ArgumentException("At least one input is required.");

                _channels = _inputs[0].Channels;
                _sampleRate = _inputs[0].SampleRate;

                SetupSink();
                ConfigureGraph();

                _outputFrame = ffmpeg.av_frame_alloc();
                if (_outputFrame == null)
                    throw new InvalidOperationException("Failed to allocate FFmpeg frame.");

                if (_mode == FilterExecutionMode.ReturnAudio)
                    _accumulator = new AudioAccumulator(_channels, _sampleRate);
            }

            public void Process()
            {
                foreach (var input in _inputs)
                {
                    SendAllFrames(input);
                    // signal EOF on this source
                    FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame(input.Source, null), nameof(ffmpeg.av_buffersrc_add_frame));
                }

                Drain(final: true);
            }

            public AudioBuffer? BuildOutput() => _accumulator?.ToBuffer();

            private void SendAllFrames(GraphInputState state)
            {
                int total = state.Buffer.Length;
                int offset = 0;
                long pts = 0;

                while (offset < total)
                {
                    int take = Math.Min(ChunkSamples, total - offset);
                    SendFrame(state, offset, take, pts);
                    pts += take;
                    offset += take;
                    Drain();
                }
            }

            private void SendFrame(GraphInputState state, int offset, int sampleCount, long pts)
            {
                // Reuse a single buffer per input: allocate once, then only make-writable per chunk.
                FfUtils.ThrowIfError(ffmpeg.av_frame_make_writable(state.Frame), nameof(ffmpeg.av_frame_make_writable));

                state.Frame->nb_samples = sampleCount;
                state.Frame->pts = pts;

                // Interleave from planar -> interleaved float
                float* dst = (float*)state.Frame->data[0];
                var planar = state.Buffer.Planar;

                // interleave: [L0,R0, L1,R1, ...]
                for (int i = 0; i < sampleCount; i++)
                {
                    int baseIndex = i * state.Channels;
                    for (int ch = 0; ch < state.Channels; ch++)
                        dst[baseIndex + ch] = planar[ch][offset + i];
                }

                FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame(state.Source, state.Frame), nameof(ffmpeg.av_buffersrc_add_frame));
            }

            private void Drain(bool final = false)
            {
                while (true)
                {
                    int ret = ffmpeg.av_buffersink_get_frame(_sink, _outputFrame);
                    if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                        break;

                    FfUtils.ThrowIfError(ret, nameof(ffmpeg.av_buffersink_get_frame));

                    if (_accumulator is not null)
                        _accumulator.Add(_outputFrame);

                    ffmpeg.av_frame_unref(_outputFrame);
                }

                if (final)
                    ffmpeg.av_frame_unref(_outputFrame);
            }

            private GraphInputState[] CreateInputs(IReadOnlyList<GraphInput> inputs)
            {
                var result = new GraphInputState[inputs.Count];
                for (int i = 0; i < inputs.Count; i++)
                {
                    var input = inputs[i];
                    if (input.Buffer is null)
                        throw new ArgumentNullException(nameof(inputs), "Input buffer cannot be null.");

                    string label = string.IsNullOrWhiteSpace(input.Label) ? $"in{i}" : input.Label;
                    result[i] = SetupSource(label, input.Buffer);
                }
                return result;
            }

            private GraphInputState SetupSource(string label, AudioBuffer buffer)
            {
                if (buffer.SampleRate <= 0)
                    throw new InvalidOperationException("Input buffer has an invalid sample rate (<= 0).");

                var abuffer = ffmpeg.avfilter_get_by_name("abuffer");
                if (abuffer == null)
                    throw new InvalidOperationException("FFmpeg 'abuffer' filter not found.");

                var src = ffmpeg.avfilter_graph_alloc_filter(_graph, abuffer, label);
                if (src == null)
                    throw new InvalidOperationException("Failed to allocate audio buffer source filter.");

                // --- Initialize abuffer via args string for maximum compatibility (FFmpeg 7.1.1) ---
                AVChannelLayout layout = FfUtils.CreateDefaultChannelLayout(1);
                long mask = ffmpeg.av_buffersink_get_ch_layout(_sink, &layout);
                var fmtName = ffmpeg.av_get_sample_fmt_name(AVSampleFormat.AV_SAMPLE_FMT_FLT)
                             ?? throw new InvalidOperationException("Failed to resolve sample format name.");

                string args =
                    $"time_base=1/{buffer.SampleRate}:sample_rate={buffer.SampleRate}:sample_fmt={fmtName}:channel_layout=0x{mask:X}";

                FfUtils.ThrowIfError(ffmpeg.avfilter_init_str(src, args), nameof(ffmpeg.avfilter_init_str));

                // --- Preallocate a reusable frame once per input (interleaved float, capacity = ChunkSamples) ---
                var frame = ffmpeg.av_frame_alloc(); 
                frame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_FLT;
                frame->sample_rate = buffer.SampleRate;
                frame->nb_samples = ChunkSamples;

                ffmpeg.av_channel_layout_uninit(&frame->ch_layout);
                ffmpeg.av_channel_layout_default(&frame->ch_layout, buffer.Channels);

                FfUtils.ThrowIfError(ffmpeg.av_frame_get_buffer(frame, 0), nameof(ffmpeg.av_frame_get_buffer));

                return new GraphInputState(label, buffer, src, frame);
            }

            private void SetupSink()
            {
                var abuffersink = ffmpeg.avfilter_get_by_name("abuffersink");
                if (abuffersink == null)
                    throw new InvalidOperationException("FFmpeg 'abuffersink' filter not found.");

                _sink = ffmpeg.avfilter_graph_alloc_filter(_graph, abuffersink, "sink");

                const int O = ffmpeg.AV_OPT_SEARCH_CHILDREN;

                // --- Constrain the sink via lists (must be set BEFORE init) ---

                // sample_fmts (prefer interleaved float)
                int* fmts = stackalloc int[2];
                fmts[0] = (int)AVSampleFormat.AV_SAMPLE_FMT_FLT;
                fmts[1] = (int)AVSampleFormat.AV_SAMPLE_FMT_NONE;
                FfUtils.ThrowIfError(
                    ffmpeg.av_opt_set_int(_sink, "sample_fmts", fmts, AVSampleFormat.AV_SAMPLE_FMT_NONE, O),
                    "av_opt_set_int_list(sample_fmts)");

                // channel_layouts (mask list matching the first input's channel count)
                long* layouts = stackalloc long[2];
                layouts[0] = ffmpeg.av_get_default_channel_layout(_channels);
                layouts[1] = -1;
                FfUtils.ThrowIfError(
                    ffmpeg.av_opt_set_int_list(_sink, "channel_layouts", layouts, -1, O),
                    "av_opt_set_int_list(channel_layouts)");

                // sample_rates
                int* rates = stackalloc int[2];
                rates[0] = _sampleRate;
                rates[1] = -1;
                FfUtils.ThrowIfError(
                    ffmpeg.av_opt_set_int_list(_sink, "sample_rates", rates, -1, O),
                    "av_opt_set_int_list(sample_rates)");

                // Now init the sink
                FfUtils.ThrowIfError(ffmpeg.avfilter_init_str(_sink, null), nameof(ffmpeg.avfilter_init_str));
            }

            private void ConfigureGraph()
            {
                AVFilterInOut* outputs = null;
                AVFilterInOut* inputs = ffmpeg.avfilter_inout_alloc();

                try
                {
                    if (inputs == null)
                        throw new InvalidOperationException("Failed to allocate FFmpeg filter in/out structures.");

                    // Build outputs list from our sources (reverse order, as required)
                    for (int i = _inputs.Length - 1; i >= 0; i--)
                    {
                        var inout = ffmpeg.avfilter_inout_alloc();
                        if (inout == null)
                            throw new InvalidOperationException("Failed to allocate FFmpeg filter in/out structures.");

                        inout->name = ffmpeg.av_strdup(_inputs[i].Label);
                        inout->filter_ctx = _inputs[i].Source;
                        inout->pad_idx = 0;
                        inout->next = outputs;
                        outputs = inout;
                    }

                    // Sink endpoint
                    inputs->name = ffmpeg.av_strdup("out");
                    inputs->filter_ctx = _sink;
                    inputs->pad_idx = 0;
                    inputs->next = null;

                    // Parse user graph and connect our endpoints
                    FfUtils.ThrowIfError(ffmpeg.avfilter_graph_parse_ptr(_graph, _filterSpec, &inputs, &outputs, null),
                                         nameof(ffmpeg.avfilter_graph_parse_ptr));
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
                        ffmpeg.avfilter_inout_free(&inputs);
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
                    _channels[i] = new List<float>(8192);

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

                for (int i = 0; i < samples; i++)
                {
                    int baseIndex = i * channels;
                    for (int ch = 0; ch < channels; ch++)
                        _channels[ch].Add(data[baseIndex + ch]);
                }
            }

            public AudioBuffer ToBuffer()
            {
                if (!_sampleRateSet)
                    throw new InvalidOperationException("FFmpeg filter graph did not report an output sample rate.");

                int length = _channels.Length > 0 ? _channels[0].Count : 0;
                var buffer = new AudioBuffer(_channels.Length, _sampleRate, length);

                for (int ch = 0; ch < _channels.Length; ch++)
                    _channels[ch].CopyTo(buffer.Planar[ch], 0);

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
}
