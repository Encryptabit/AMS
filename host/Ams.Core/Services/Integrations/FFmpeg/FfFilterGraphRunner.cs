using Ams.Core.Artifacts;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg
{
    internal static class FfFilterGraphRunner
    {
        internal readonly record struct GraphInput(string Label, AudioBuffer Buffer);

        internal unsafe interface IAudioFrameSink : IDisposable
        {
            void Initialize(AudioBufferMetadata? templateMetadata, int sampleRate, int channels);
            void Consume(AVFrame* frame);
            void Complete();
        }

        internal enum FilterExecutionMode
        {
            ReturnAudio,
            DiscardOutput,
        }

        public static AudioBuffer Apply(AudioBuffer input, string filterSpec)
        {
            var inputs = new[] { new GraphInput("main", input) };
            return ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio, null)
                   ?? throw new InvalidOperationException("FFmpeg filter graph did not produce audio output.");
        }

        public static AudioBuffer Apply(IReadOnlyList<GraphInput> inputs, string filterSpec)
        {
            if (inputs is null || inputs.Count == 0)
                throw new ArgumentException("At least one input is required.", nameof(inputs));

            return ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio, null)
                   ?? throw new InvalidOperationException("FFmpeg filter graph did not produce audio output.");
        }

        public static void Execute(AudioBuffer input, string filterSpec, FilterExecutionMode mode)
        {
            var inputs = new[] { new GraphInput("main", input) };
            ExecuteInternal(inputs, filterSpec, mode, null);
        }

        public static void Execute(IReadOnlyList<GraphInput> inputs, string filterSpec, FilterExecutionMode mode)
        {
            if (inputs is null || inputs.Count == 0)
                throw new ArgumentException("At least one input is required.", nameof(inputs));

            ExecuteInternal(inputs, filterSpec, mode, null);
        }

        public static void Stream(IReadOnlyList<GraphInput> inputs, string filterSpec, IAudioFrameSink sink)
        {
            if (inputs is null || inputs.Count == 0)
                throw new ArgumentException("At least one input is required.", nameof(inputs));

            if (sink is null)
                throw new ArgumentNullException(nameof(sink));

            ExecuteInternal(inputs, filterSpec, FilterExecutionMode.DiscardOutput, sink);
        }

        private static AudioBuffer? ExecuteInternal(IReadOnlyList<GraphInput> inputs, string filterSpec,
            FilterExecutionMode mode, IAudioFrameSink? frameSink)
        {
            using var executor = new FilterGraphExecutor(inputs, filterSpec, mode, frameSink);
            executor.Process();
            return executor.BuildOutput();
        }

        private sealed unsafe class FilterGraphExecutor : IDisposable
        {
            // Tuneable: larger chunk = fewer calls, smaller chunk = lower latency.
            private const int ChunkSamples = 4096;
            private const int BufferSrcFlagKeepRef = 8;

            private readonly GraphInputState[] _inputs;
            private readonly string _filterSpec;
            private readonly FilterExecutionMode _mode;
            private readonly AudioBufferMetadata? _primaryMetadata;
            private readonly IAudioFrameSink? _frameSink;

            private AVFilterGraph* _graph;
            private AVFilterContext* _sink;
            private AVFrame* _outputFrame;
            private AudioAccumulator? _accumulator;

            private readonly int _channels;
            private int _sampleRate;

            public FilterGraphExecutor(IReadOnlyList<GraphInput> inputs, string filterSpec, FilterExecutionMode mode,
                IAudioFrameSink? frameSink)
            {
                FfSession.EnsureFiltersAvailable();

                // Let parse/connect wire sources->spec->sink. If empty, use a no-op.
                _filterSpec = string.IsNullOrWhiteSpace(filterSpec) ? "anull" : filterSpec;
                _mode = mode;
                _frameSink = frameSink;

                _graph = ffmpeg.avfilter_graph_alloc();
                if (_graph == null)
                    throw new InvalidOperationException("Failed to allocate FFmpeg filter graph.");

                _inputs = CreateInputs(inputs);
                if (_inputs.Length == 0)
                    throw new ArgumentException("At least one input is required.");

                _primaryMetadata = inputs.Count > 0 ? inputs[0].Buffer.Metadata : null;

                _channels = _inputs[0].Channels;
                _sampleRate = _inputs[0].SampleRate;

                SetupSink();
                ConfigureGraph();
                RefreshOutputFormat();

                _outputFrame = ffmpeg.av_frame_alloc();
                if (_outputFrame == null)
                    throw new InvalidOperationException("Failed to allocate FFmpeg frame.");

                if (_frameSink != null)
                {
                    _frameSink.Initialize(_primaryMetadata, _sampleRate, _channels);
                }

                if (_mode == FilterExecutionMode.ReturnAudio && _frameSink is null)
                    _accumulator = new AudioAccumulator(_channels, _sampleRate);
            }

            public void Process()
            {
                foreach (var input in _inputs)
                {
                    SendAllFrames(input);
                    // signal EOF on this source
                    FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame(input.Source, null),
                        nameof(ffmpeg.av_buffersrc_add_frame));
                }

                Drain(final: true);
                _frameSink?.Complete();
            }

            public AudioBuffer? BuildOutput() => _accumulator?.ToBuffer(_primaryMetadata);

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
                var frame = state.Frame;
                if (frame == null)
                    throw new InvalidOperationException("Input frame has not been initialized.");

                FfUtils.ThrowIfError(ffmpeg.av_frame_make_writable(frame), nameof(ffmpeg.av_frame_make_writable));

                frame->nb_samples = sampleCount;
                frame->pts = pts;

                float* dst = (float*)frame->data[0];
                var planar = state.Buffer.Planar;

                for (int i = 0; i < sampleCount; i++)
                {
                    int baseIndex = i * state.Channels;
                    for (int ch = 0; ch < state.Channels; ch++)
                        dst[baseIndex + ch] = planar[ch][offset + i];
                }

                FfUtils.ThrowIfError(ffmpeg.av_buffersrc_add_frame_flags(state.Source, frame, BufferSrcFlagKeepRef),
                    nameof(ffmpeg.av_buffersrc_add_frame_flags));
            }

            private void Drain(bool final = false)
            {
                while (true)
                {
                    int ret = ffmpeg.av_buffersink_get_frame(_sink, _outputFrame);
                    if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                        break;

                    FfUtils.ThrowIfError(ret, nameof(ffmpeg.av_buffersink_get_frame));

                    if (_frameSink is not null)
                        _frameSink.Consume(_outputFrame);
                    else if (_accumulator is not null)
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

                var fmtName = ffmpeg.av_get_sample_fmt_name(AVSampleFormat.AV_SAMPLE_FMT_FLT)
                              ?? throw new InvalidOperationException("Failed to resolve sample format name.");

                var layoutName = buffer.Metadata?.CurrentChannelLayout ??
                                 AudioBufferMetadata.DescribeDefaultLayout(buffer.Channels);
                string args =
                    $"time_base=1/{buffer.SampleRate}:sample_rate={buffer.SampleRate}:sample_fmt={fmtName}:channel_layout={layoutName}";

                FfUtils.ThrowIfError(ffmpeg.avfilter_init_str(src, args), nameof(ffmpeg.avfilter_init_str));

                var layout = FfUtils.CloneOrDefault(null, buffer.Channels);
                var frame = ffmpeg.av_frame_alloc();
                if (frame == null)
                    throw new InvalidOperationException("Failed to allocate reusable FFmpeg frame.");

                try
                {
                    frame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_FLT;
                    frame->sample_rate = buffer.SampleRate;
                    frame->nb_samples = ChunkSamples;

                    ffmpeg.av_channel_layout_uninit(&frame->ch_layout);
                    FfUtils.ThrowIfError(ffmpeg.av_channel_layout_copy(&frame->ch_layout, &layout),
                        nameof(ffmpeg.av_channel_layout_copy));

                    FfUtils.ThrowIfError(ffmpeg.av_frame_get_buffer(frame, 0), nameof(ffmpeg.av_frame_get_buffer));
                }
                catch
                {
                    ffmpeg.av_frame_free(&frame);
                    ffmpeg.av_channel_layout_uninit(&layout);
                    throw;
                }

                return new GraphInputState(label, buffer, src, layout, frame);
            }


            private void SetupSink()
            {
                var abuffersink = ffmpeg.avfilter_get_by_name("abuffersink");
                if (abuffersink == null)
                    throw new InvalidOperationException("FFmpeg 'abuffersink' filter not found.");

                _sink = ffmpeg.avfilter_graph_alloc_filter(_graph, abuffersink, "sink");
                if (_sink == null)
                    throw new InvalidOperationException("Failed to allocate audio buffer sink filter.");

                ConfigureSinkFormat();

                FfUtils.ThrowIfError(ffmpeg.avfilter_init_str(_sink, null), nameof(ffmpeg.avfilter_init_str));
            }

            private void ConfigureSinkFormat()
            {
                ConfigureIntOption("sample_fmts", (int)AVSampleFormat.AV_SAMPLE_FMT_FLT);
                ConfigureChannelLayouts();
            }

            private void ConfigureChannelLayouts()
            {
                var layoutName = _primaryMetadata?.CurrentChannelLayout
                                 ?? AudioBufferMetadata.DescribeDefaultLayout(_channels);
                if (string.IsNullOrWhiteSpace(layoutName))
                {
                    layoutName = "mono";
                }

                var layoutResult = ffmpeg.av_opt_set(
                    _sink,
                    "ch_layouts",
                    layoutName,
                    ffmpeg.AV_OPT_SEARCH_CHILDREN);

                if (layoutResult < 0)
                {
                    var msg = FfUtils.FormatError(layoutResult);
                    throw new InvalidOperationException($"Failed to configure sink channel layout: {msg}");
                }
            }

            private void ConfigureIntOption(string optionName, int value)
            {
                int* buffer = stackalloc int[2];
                buffer[0] = value;
                buffer[1] = -1;

                var result = ffmpeg.av_opt_set_bin(
                    _sink,
                    optionName,
                    (byte*)buffer,
                    sizeof(int) * 2,
                    ffmpeg.AV_OPT_SEARCH_CHILDREN);

                if (result < 0)
                {
                    var msg = FfUtils.FormatError(result);
                    throw new InvalidOperationException($"Failed to configure sink option '{optionName}': {msg}");
                }
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
                    var parseCode = ffmpeg.avfilter_graph_parse_ptr(_graph, _filterSpec, &inputs, &outputs, null);
                    if (parseCode < 0)
                    {
                        var msg = FfUtils.FormatError(parseCode);
                        throw new InvalidOperationException($"Failed to parse filter graph '{_filterSpec}': {msg}");
                    }

                    var configCode = ffmpeg.avfilter_graph_config(_graph, null);
                    if (configCode < 0)
                    {
                        var msg = FfUtils.FormatError(configCode);
                        throw new InvalidOperationException($"Failed to configure filter graph '{_filterSpec}': {msg}");
                    }
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

            private void RefreshOutputFormat()
            {
                if (_sink == null)
                {
                    return;
                }

                var sinkRate = ffmpeg.av_buffersink_get_sample_rate(_sink);
                if (sinkRate > 0)
                {
                    _sampleRate = sinkRate;
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
                        input.Dispose();
                    }
                }

                _frameSink?.Dispose();
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

            public AudioBuffer ToBuffer(AudioBufferMetadata? templateMetadata = null)
            {
                if (!_sampleRateSet)
                    throw new InvalidOperationException("FFmpeg filter graph did not report an output sample rate.");

                int channelCount = _channels.Length;
                int length = channelCount > 0 ? _channels[0].Count : 0;

                AudioBufferMetadata metadata;
                if (templateMetadata is not null)
                {
                    var layout = templateMetadata.CurrentChannelLayout ??
                                 AudioBufferMetadata.DescribeDefaultLayout(channelCount);
                    metadata = templateMetadata.WithCurrentStream(_sampleRate, channelCount, "fltp", layout);
                }
                else
                {
                    metadata = AudioBufferMetadata.CreateDefault(_sampleRate, channelCount);
                }

                var buffer = new AudioBuffer(channelCount, _sampleRate, length, metadata);

                for (int ch = 0; ch < channelCount; ch++)
                    _channels[ch].CopyTo(buffer.Planar[ch], 0);

                return buffer;
            }
        }

        private sealed unsafe class GraphInputState
        {
            public GraphInputState(
                string label,
                AudioBuffer buffer,
                AVFilterContext* source,
                AVChannelLayout layout,
                AVFrame* frame)
            {
                Label = label;
                Buffer = buffer;
                Source = source;
                Layout = layout;
                SampleRate = buffer.SampleRate;
                Channels = buffer.Channels;
                Frame = frame;
            }

            public string Label { get; }
            public AudioBuffer Buffer { get; }
            public AVFilterContext* Source { get; }
            public AVChannelLayout Layout;
            public int SampleRate { get; }
            public int Channels { get; }
            public AVFrame* Frame { get; private set; }

            public void Dispose()
            {
                if (Frame != null)
                {
                    var frame = Frame;
                    ffmpeg.av_frame_free(&frame);
                    Frame = null;
                }

                fixed (AVChannelLayout* layoutPtr = &Layout)
                {
                    ffmpeg.av_channel_layout_uninit(layoutPtr);
                }
            }
        }
    }
}