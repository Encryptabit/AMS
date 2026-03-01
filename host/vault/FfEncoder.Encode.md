---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 13
fan_in: 2
fan_out: 12
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::Encode
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Encodes an in-memory `AudioBuffer` to WAV/PCM through FFmpeg using either dynamic-buffer or custom-stream output sinks.**

This private method is the full FFmpeg WAV-encoding pipeline: it validates inputs, initializes FFmpeg, resolves target PCM codec/sample format from options, configures `AVFormatContext`/`AVCodecContext`/stream, and sets sink-specific IO (`SetupIo`). It builds a resampler from input planar float (`AV_SAMPLE_FMT_FLTP`) to target format/rate/layout, pins managed channel arrays, allocates an output frame, then encodes buffered chunks via `EncodeBuffer`, flushes encoder state (`avcodec_send_frame(null)` + `DrainEncoder`), writes trailer, and finalizes IO output handling. The implementation includes exhaustive unmanaged cleanup in `finally` (unpinning, freeing resampler/frame/codec, channel-layout uninit, IO cleanup, format-context free) to avoid leaks across both success and failure paths.


#### [[FfEncoder.Encode]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void Encode(AudioBuffer buffer, Stream output, AudioEncodeOptions options, FfEncoder.EncoderSink sink)
```

**Calls ->**
- [[FfEncoder.AllocateFrame]]
- [[FfEncoder.CleanupIo]]
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.FinalizeIo]]
- [[FfEncoder.PinChannels]]
- [[FfEncoder.ResolveEncoding]]
- [[FfEncoder.SetupIo]]
- [[FfEncoder.UnpinChannels]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.CreateDefaultChannelLayout]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.EncodeToCustomStream]]
- [[FfEncoder.EncodeToDynamicBuffer]]

