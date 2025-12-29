# FFmpeg Call Graph Documentation

**Purpose:** Map call chains from CLI commands through FFmpeg entry points to native functions.

## High-Level Architecture

```
+----------------+     +-------------------+     +------------------+     +---------------+
|  CLI Commands  | --> |  AudioProcessor   | --> |  FFmpeg Wrappers | --> |  Native Libs  |
|  (Ams.Cli)     |     |  (Ams.Core)       |     |  (Ff*.cs)        |     |  (libav*)     |
+----------------+     +-------------------+     +------------------+     +---------------+
```

## Call Chain Diagrams

### 1. Audio Decoding Chain

```
CLI/Application
    |
    v
AudioProcessor.Decode(path, options)
    |
    v
FfDecoder.Decode(path, options)
    |
    +-> FfSession.EnsureInitialized()
    |       |
    |       +-> avformat_network_init()
    |       +-> av_log_set_level()
    |
    +-> avformat_open_input()
    +-> avformat_find_stream_info()
    +-> av_find_best_stream()
    +-> avcodec_find_decoder()
    +-> avcodec_alloc_context3()
    +-> avcodec_parameters_to_context()
    +-> avcodec_open2()
    |
    +-> [If resampling needed]
    |       +-> swr_alloc()
    |       +-> swr_alloc_set_opts2()
    |       +-> swr_init()
    |
    +-> [Decode loop]
    |       +-> av_read_frame()
    |       +-> avcodec_send_packet()
    |       +-> avcodec_receive_frame()
    |       +-> swr_convert() [if resampling]
    |
    +-> [Cleanup]
            +-> swr_free()
            +-> avcodec_free_context()
            +-> avformat_close_input()
            +-> av_channel_layout_uninit()
```

### 2. Audio Encoding Chain

```
CLI/Application
    |
    v
AudioProcessor.EncodeWav(path, buffer, options)
    |
    v
FfEncoder.EncodeToCustomStream(buffer, stream, options)
    |
    v
FfEncoder.Encode(buffer, output, options, EncoderSink)
    |
    +-> FfSession.EnsureInitialized()
    |
    +-> avcodec_find_encoder()
    +-> avformat_alloc_output_context2()
    +-> avformat_new_stream()
    +-> avcodec_alloc_context3()
    +-> av_channel_layout_default()
    +-> avcodec_open2()
    +-> avcodec_parameters_from_context()
    |
    +-> SetupIo() [for custom stream]
    |       +-> avio_alloc_context()
    |       +-> av_malloc()
    |
    +-> avformat_write_header()
    |
    +-> [Resample setup]
    |       +-> swr_alloc()
    |       +-> swr_alloc_set_opts2()
    |       +-> swr_init()
    |
    +-> [Encode loop]
    |       +-> swr_convert()
    |       +-> av_frame_make_writable()
    |       +-> avcodec_send_frame()
    |       +-> avcodec_receive_packet()
    |       +-> av_write_frame()
    |
    +-> av_write_trailer()
    |
    +-> [Cleanup]
            +-> swr_free()
            +-> av_frame_free()
            +-> avcodec_free_context()
            +-> avformat_free_context()
```

### 3. Filter Graph Execution Chain

```
CLI/Application
    |
    v
AudioProcessor.Resample(buffer, rate) / Trim() / FadeIn() / etc.
    |
    v
FfFilterGraph.FromBuffer(buffer)
    .Resample(...) / .Custom(...) / etc.
    .ToBuffer()
    |
    v
FfFilterGraphRunner.Apply(inputs, filterSpec)
    |
    v
FilterGraphExecutor (constructor)
    |
    +-> FfSession.EnsureFiltersAvailable()
    |       |
    |       +-> avfilter_graph_alloc() [probe]
    |       +-> avfilter_graph_free()
    |
    +-> avfilter_graph_alloc()
    |
    +-> CreateInputs() [for each input buffer]
    |       +-> avfilter_get_by_name("abuffer")
    |       +-> avfilter_graph_alloc_filter()
    |       +-> avfilter_init_str()
    |       +-> av_frame_alloc()
    |       +-> av_channel_layout_copy()
    |       +-> av_frame_get_buffer()
    |
    +-> SetupSink()
    |       +-> avfilter_get_by_name("abuffersink")
    |       +-> avfilter_graph_alloc_filter()
    |       +-> av_opt_set_bin() [sample formats]
    |       +-> av_opt_set() [channel layouts]
    |       +-> avfilter_init_str()
    |
    +-> ConfigureGraph()
    |       +-> avfilter_inout_alloc()
    |       +-> av_strdup()
    |       +-> avfilter_graph_parse_ptr()
    |       +-> avfilter_graph_config()
    |       +-> avfilter_inout_free()
    |
    +-> av_frame_alloc() [output frame]
    |
    v
FilterGraphExecutor.Process()
    |
    +-> [For each input]
    |       +-> SendAllFrames()
    |       |       +-> av_frame_make_writable()
    |       |       +-> av_buffersrc_add_frame_flags()
    |       |       +-> Drain()
    |       +-> av_buffersrc_add_frame(null) [EOF]
    |
    +-> Drain(final: true)
    |       +-> av_buffersink_get_frame()
    |       +-> [Accumulate output]
    |       +-> av_frame_unref()
    |
    +-> BuildOutput()
            +-> Create AudioBuffer from accumulated samples
```

### 4. Log Capture Chain

```
FfFilterGraph.CaptureLogs()
    |
    v
FfLogCapture.Capture(action)
    |
    +-> FfSession.EnsureFiltersAvailable()
    +-> av_log_get_level()
    +-> av_log_set_level(AV_LOG_INFO)
    +-> av_log_set_callback(LogCallback)
    |
    +-> [Execute action - filter graph runs]
    |       |
    |       +-> LogCallback() [called by FFmpeg]
    |               +-> av_log_format_line()
    |               +-> [Collect to thread-local list]
    |
    +-> av_log_set_level(previous)
    +-> av_log_set_callback(null)
    |
    v
Return collected log lines
```

## CLI Command to FFmpeg Mapping

### dsp Command

```
dsp filter-chain run --input audio.wav --filters highpass,lowpass
    |
    +-> AudioProcessor.Decode(audio.wav)
    |       +-> FfDecoder.Decode()
    |
    +-> FfFilterGraph.FromBuffer(buffer)
    |       .HighPass(...)
    |       .LowPass(...)
    |       .ToBuffer() or .StreamToWave()
    |
    +-> [If --save]
            +-> FfEncoder.EncodeToCustomStream()
```

### pipeline Command (Indirect)

```
pipeline run --book ... --audio ... --chapter ...
    |
    +-> [ASR Stage]
    |       +-> AsrProcessor.TranscribeFileAsync()
    |               +-> AudioProcessor.Decode()  --> FfDecoder
    |               +-> AudioProcessor.Resample() --> FfFilterGraphRunner
    |
    +-> [Various alignment stages - no direct FFmpeg]
    |
    +-> [Treated copy stage]
            +-> [File copy, no FFmpeg currently]
```

### validate Command

```
validate chapter --chapter ...
    |
    +-> ValidationService (various checks)
            +-> May use AudioProcessor for audio validation
                    +-> FfDecoder / FfFilterGraphRunner
```

## Native Library Dependencies

### libavformat (Container I/O)
- `avformat_open_input` - Open media file
- `avformat_find_stream_info` - Probe stream details
- `avformat_close_input` - Close file
- `avformat_alloc_output_context2` - Create output context
- `avformat_new_stream` - Add stream to output
- `avformat_write_header` - Write container header
- `av_read_frame` - Read encoded packet
- `av_write_frame` - Write encoded packet
- `av_write_trailer` - Finalize container
- `avformat_network_init` - Initialize network protocols

### libavcodec (Codec Operations)
- `avcodec_find_decoder` - Find decoder by ID
- `avcodec_find_encoder` - Find encoder by ID
- `avcodec_alloc_context3` - Create codec context
- `avcodec_free_context` - Free codec context
- `avcodec_parameters_to_context` - Copy params to context
- `avcodec_parameters_from_context` - Copy params from context
- `avcodec_open2` - Open codec
- `avcodec_send_packet` - Send packet to decoder
- `avcodec_receive_frame` - Get decoded frame
- `avcodec_send_frame` - Send frame to encoder
- `avcodec_receive_packet` - Get encoded packet
- `avcodec_get_name` - Get codec name string

### libavfilter (Filter Graphs)
- `avfilter_graph_alloc` - Create filter graph
- `avfilter_graph_free` - Free filter graph
- `avfilter_get_by_name` - Find filter by name
- `avfilter_graph_alloc_filter` - Add filter to graph
- `avfilter_init_str` - Initialize filter with args
- `avfilter_graph_parse_ptr` - Parse filter string
- `avfilter_graph_config` - Validate and configure graph
- `avfilter_inout_alloc/free` - In/out endpoint management
- `av_buffersrc_add_frame` - Send frame to source
- `av_buffersrc_add_frame_flags` - Send frame with flags
- `av_buffersink_get_frame` - Get frame from sink
- `av_buffersink_get_sample_rate` - Query output format

### libswresample (Resampling)
- `swr_alloc` - Allocate resampler
- `swr_alloc_set_opts2` - Configure resampler
- `swr_init` - Initialize resampler
- `swr_convert` - Convert samples
- `swr_free` - Free resampler
- `swr_get_delay` - Get buffered samples

### libavutil (Utilities)
- `av_frame_alloc/free` - Frame memory management
- `av_frame_make_writable` - Ensure frame is writable
- `av_frame_unref` - Unreference frame data
- `av_frame_get_buffer` - Allocate frame buffers
- `av_packet_alloc/free` - Packet memory management
- `av_packet_unref` - Unreference packet data
- `av_packet_rescale_ts` - Rescale timestamps
- `av_channel_layout_default/copy/uninit` - Channel layout ops
- `av_samples_alloc_array_and_samples` - Audio buffer alloc
- `av_malloc/free/freep` - Memory allocation
- `av_strerror` - Error to string
- `av_log_set_level/get_level` - Log level control
- `av_log_set_callback` - Custom log handler
- `av_log_format_line` - Format log message
- `av_dict_get` - Dictionary access
- `av_strdup` - String duplication
- `av_opt_set/av_opt_set_bin` - Option setting
- `av_q2d` - Rational to double
- `av_rescale_rnd` - Scaled arithmetic

## FFmpeg Filter Usage Summary

| Filter | Used By | Purpose |
|--------|---------|---------|
| `abuffer` | FfFilterGraphRunner | Source input |
| `abuffersink` | FfFilterGraphRunner | Output sink |
| `aformat` | FfFilterGraph | Format negotiation |
| `aresample` | FfFilterGraph | Sample rate conversion |
| `highpass` | FfFilterGraph | High-pass filter |
| `lowpass` | FfFilterGraph | Low-pass filter |
| `deesser` | FfFilterGraph | De-essing |
| `afftdn` | FfFilterGraph | FFT-based denoise |
| `arnndn` | FfFilterGraph | Neural denoise |
| `acompressor` | FfFilterGraph | Dynamic compression |
| `alimiter` | FfFilterGraph | Peak limiting |
| `loudnorm` | FfFilterGraph | EBU R128 normalization |
| `dynaudnorm` | FfFilterGraph | Dynamic normalization |
| `silencedetect` | AudioProcessor | Silence detection |
| `silenceremove` | FfFilterGraph | Silence trimming |
| `astats` | FfFilterGraph | Audio statistics |
| `ebur128` | FfFilterGraph | Loudness measurement |
| `atrim` | AudioProcessor | Time-based trimming |
| `afade` | AudioProcessor | Fade in/out |
| `volume` | FfFilterGraph | Volume adjustment |
| `asetpts` | AudioProcessor | PTS reset after trim |
| `anull` | FfFilterGraph | No-op passthrough |
| `ashowinfo` | FfFilterGraph | Debug output |
| `asetnsamples` | FfFilterGraph | Fixed window size |
| `aspectralstats` | FfFilterGraph | Spectral analysis |
