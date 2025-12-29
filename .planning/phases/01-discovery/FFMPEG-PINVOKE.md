# FFmpeg P/Invoke and Entry Points Documentation

**Purpose:** Document FFmpeg interop patterns, unsafe code blocks, and public entry points.

**Architecture Note:** AMS uses `FFmpeg.AutoGen` NuGet package which provides managed P/Invoke wrappers. There are **no direct `[DllImport]` declarations** in AMS source code. All native function calls go through FFmpeg.AutoGen's pre-generated bindings (`FFmpeg.AutoGen.ffmpeg` static class).

## Interop Architecture

```
+---------------------+
|  AMS Application    |
+---------------------+
         |
         v
+---------------------+
|  Ams.Core FFmpeg    |  <- Unsafe code here
|  (FfDecoder, etc.)  |
+---------------------+
         |
         v
+---------------------+
|  FFmpeg.AutoGen     |  <- P/Invoke generated bindings
|  (NuGet package)    |
+---------------------+
         |
         v
+---------------------+
|  Native FFmpeg DLLs |  <- avcodec, avformat, avfilter, etc.
+---------------------+
```

## Unsafe Code Blocks by File

### FfDecoder.cs

**Class declaration:** `internal static unsafe class FfDecoder`

| Method | Unsafe Operations | Native Functions Used |
|--------|------------------|----------------------|
| `Probe(string path)` | Pointer manipulation for format/stream/codec contexts | `avformat_open_input`, `avformat_find_stream_info`, `av_find_best_stream`, `avcodec_get_name`, `av_q2d`, `avformat_close_input` |
| `Decode(string path, options)` | Frame/packet allocation, resampler setup, sample buffer manipulation | `avformat_open_input`, `avformat_find_stream_info`, `av_find_best_stream`, `avcodec_find_decoder`, `avcodec_alloc_context3`, `avcodec_parameters_to_context`, `avcodec_open2`, `swr_alloc`, `swr_alloc_set_opts2`, `swr_init`, `av_read_frame`, `avcodec_send_packet`, `avcodec_receive_frame`, `swr_convert`, `av_packet_alloc/free`, `av_frame_alloc/free`, `avcodec_free_context`, `avformat_close_input`, `swr_free`, `av_channel_layout_uninit` |
| `ResampleInto(...)` | Float pointer casting for sample data | `swr_convert` |
| `AppendSamples(...)` | Direct pointer access to frame extended_data | - |
| `ReadTags(...)` | Dictionary entry pointer iteration | `av_dict_get` |
| `ResampleScratch.Rent(...)` | Sample buffer allocation | `av_samples_alloc_array_and_samples`, `av_free` |
| `FfPacket` (inner class) | Packet lifecycle management | `av_packet_alloc`, `av_packet_unref`, `av_packet_free` |
| `FfFrame` (inner class) | Frame lifecycle management | `av_frame_alloc`, `av_frame_free` |

### FfEncoder.cs

**Class declaration:** `internal static unsafe class FfEncoder`

| Method | Unsafe Operations | Native Functions Used |
|--------|------------------|----------------------|
| `Encode(...)` | Format/codec context setup, custom I/O, frame encoding | `avcodec_find_encoder`, `avformat_alloc_output_context2`, `avformat_new_stream`, `avcodec_alloc_context3`, `av_channel_layout_uninit/default`, `avcodec_open2`, `avcodec_parameters_from_context`, `avformat_write_header`, `swr_alloc`, `swr_alloc_set_opts2`, `swr_init`, `av_frame_alloc`, `av_write_trailer`, `swr_free`, `av_frame_free`, `avcodec_free_context`, `avformat_free_context` |
| `EncodeBuffer(...)` | Stackalloc for source pointers, frame buffer management | `swr_convert`, `av_frame_make_writable`, `avcodec_send_frame` |
| `DrainEncoder(...)` | Packet processing loop | `av_packet_alloc/free`, `avcodec_receive_packet`, `av_packet_rescale_ts`, `av_write_frame` |
| `SetupIo(...)` | Custom AVIO context for stream writing | `avio_open_dyn_buf`, `av_malloc`, `avio_alloc_context` |
| `FinalizeIo(...)` | Dynamic buffer extraction | `avio_close_dyn_buf`, `av_free` |
| `AvioWritePacket(...)` | AVIO write callback (GCHandle for stream) | - |
| `StreamingEncoderSink` | Full streaming encoder context | All encoder functions + frame management |

### FfFilterGraphRunner.cs

**Class declaration:** `internal static class FfFilterGraphRunner` (contains unsafe inner classes)

| Method/Class | Unsafe Operations | Native Functions Used |
|--------------|------------------|----------------------|
| `FilterGraphExecutor` (inner) | Full filter graph lifecycle | `avfilter_graph_alloc`, `avfilter_get_by_name`, `avfilter_graph_alloc_filter`, `avfilter_init_str`, `av_frame_alloc`, `av_channel_layout_copy`, `av_frame_get_buffer`, `avfilter_inout_alloc`, `av_strdup`, `avfilter_graph_parse_ptr`, `avfilter_graph_config`, `av_buffersrc_add_frame`, `av_buffersrc_add_frame_flags`, `av_buffersink_get_frame`, `av_frame_unref`, `av_frame_free`, `avfilter_graph_free`, `avfilter_inout_free`, `av_channel_layout_uninit` |
| `SendFrame(...)` | Direct float pointer writing to frame data | - |
| `AudioAccumulator` (inner) | Frame data extraction | - |
| `GraphInputState` (inner) | Per-input state with frame pointers | - |
| `ConfigureSinkFormat(...)` | Binary option configuration | `av_opt_set_bin`, `av_opt_set` |

### FfLogCapture.cs

**Class declaration:** `internal static unsafe class FfLogCapture`

| Method | Unsafe Operations | Native Functions Used |
|--------|------------------|----------------------|
| `Capture(Action)` | Log callback registration | `av_log_set_callback`, `av_log_get_level`, `av_log_set_level` |
| `LogCallback(...)` | Stack-allocated buffer, va_list processing | `av_log_format_line` |

**Custom Delegate:**
```csharp
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
private unsafe delegate void AvLogDelegate(void* ptr, int level, string format, byte* vl);
```

### FfUtils.cs

**Class declaration:** `internal static unsafe class FfUtils`

| Method | Unsafe Operations | Native Functions Used |
|--------|------------------|----------------------|
| `ThrowIfError(...)` | Error string formatting | - |
| `FormatError(...)` | Stack-allocated error buffer | `av_strerror` |
| `CleanupThrowIfError(...)` | Resource cleanup on error | `av_freep`, `avio_context_free`, `av_channel_layout_uninit`, `avcodec_free_context`, `avformat_free_context` |
| `CloneOrDefault(...)` | Channel layout pointer handling | `av_channel_layout_copy`, `av_channel_layout_default` |
| `CreateDefaultChannelLayout(...)` | Channel layout creation | `av_channel_layout_default` |
| `CheckSampleFormat(...)` | Codec capability checking | - |
| `SelectSampleRate(...)` | Sample rate negotiation | `avcodec_get_supported_config`, `av_log` |
| `SelectChannelLayout(...)` | Channel layout selection | - |
| `ComputeResampleOutputSamples(...)` | Resampler delay calculation | `swr_get_delay`, `av_rescale_rnd` |

### FfSession.cs

**Method-level unsafe:** `EnsureFilterProbe()` contains `unsafe` block

| Method | Unsafe Operations | Native Functions Used |
|--------|------------------|----------------------|
| `EnsureInitialized()` | Global FFmpeg init | `av_log_set_level`, `avformat_network_init` |
| `EnsureFilterProbe()` | Filter availability test | `avfilter_graph_alloc`, `avfilter_graph_free` |

## Public Entry Points

### AudioProcessor (Primary API)

| Method | Description | FFmpeg Operations |
|--------|-------------|-------------------|
| `Probe(string path)` | Get audio file metadata | Format detection, stream info |
| `Decode(string path, options?)` | Decode audio to buffer | Full decode + resample |
| `EncodeWav(string path, buffer, options?)` | Write buffer to WAV | PCM encoding |
| `EncodeWavToStream(buffer, options?)` | Encode to memory stream | PCM encoding |
| `Resample(buffer, targetRate)` | Resample audio | Filter graph with aresample |
| `DetectSilence(buffer, options?)` | Find silence intervals | silencedetect filter |
| `Trim(buffer, start, end?)` | Extract time range | atrim filter |
| `FadeIn(buffer, duration)` | Apply fade-in | afade filter |
| `FadeOut(buffer, start, duration)` | Apply fade-out | afade filter |
| `AdjustVolume(buffer, gain)` | Adjust volume | volume filter |
| `NormalizeLoudness(buffer, options?)` | Two-pass loudnorm | loudnorm filter |

### FfFilterGraph (Fluent Builder)

| Method | Filter | Description |
|--------|--------|-------------|
| `AFormat(...)` | aformat | Constrain sample format/layout |
| `HighPass(freq)` | highpass | Remove low frequencies |
| `LowPass(freq)` | lowpass | Remove high frequencies |
| `DeEsser(...)` | deesser | Reduce sibilance |
| `FftDenoise(...)` | afftdn | Frequency-domain denoise |
| `NeuralDenoise(...)` | arnndn | Neural network denoise |
| `ACompressor(...)` | acompressor | Dynamic compression |
| `ALimiter(...)` | alimiter | Peak limiting |
| `LoudNorm(...)` | loudnorm | EBU R128 normalization |
| `DynaudNorm(...)` | dynaudnorm | Dynamic audio normalization |
| `SilenceRemove(...)` | silenceremove | Trim silence |
| `AStats(...)` | astats | Audio statistics |
| `EbuR128(...)` | ebur128 | Loudness measurement |
| `Resample(...)` | aresample | Sample rate conversion |
| `Gain(...)` | volume | Volume adjustment |
| `ToBuffer()` | - | Execute and return result |
| `StreamToWave(...)` | - | Execute and stream to file |
| `CaptureLogs()` | - | Execute and capture logs |

### FfSession (Initialization)

| Method | Description |
|--------|-------------|
| `EnsureInitialized()` | Initialize FFmpeg globals |
| `EnsureFiltersAvailable()` | Verify libavfilter is present |
| `FiltersAvailable` (property) | Check filter availability |

## Memory Management Patterns

### Resource Acquisition

1. **Format Context:** `avformat_open_input` -> must call `avformat_close_input`
2. **Codec Context:** `avcodec_alloc_context3` -> must call `avcodec_free_context`
3. **Frames:** `av_frame_alloc` -> must call `av_frame_free`
4. **Packets:** `av_packet_alloc` -> must call `av_packet_free`
5. **Resampler:** `swr_alloc` + `swr_init` -> must call `swr_free`
6. **Filter Graph:** `avfilter_graph_alloc` -> must call `avfilter_graph_free`
7. **Channel Layout:** Initialized -> must call `av_channel_layout_uninit`

### RAII Wrappers in AMS

- `FfPacket` - IDisposable wrapper for AVPacket
- `FfFrame` - IDisposable wrapper for AVFrame
- `ResampleScratch` - IDisposable scratch buffer for resampling
- `FilterGraphExecutor` - IDisposable filter graph executor
- `StreamingEncoderSink` - IDisposable streaming encoder

### GCHandle Usage

- `FfEncoder.SetupIo`: Pins Stream object for AVIO callback
- `FfEncoder.PinChannels`: Pins audio channel arrays during encoding

## Thread Safety

- `FfSession`: Uses static lock for initialization
- `FfLogCapture`: Uses static lock + ThreadStatic for log collection
- Filter graph execution: Single-threaded per graph instance
- All FFmpeg state is process-global; concurrent use requires separate contexts
