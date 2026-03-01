---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::SetupIo
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Initializes the FFmpeg muxer IO backend for either in-memory dynamic buffering or callback-based custom stream writing.**

This method configures FFmpeg output IO according to sink mode, either using FFmpeg’s dynamic buffer or a custom managed-stream writer callback. For `DynamicBuffer`, it opens `fmt->pb` via `avio_open_dyn_buf` with error checking. For custom streaming, it allocates an AVIO buffer (`av_malloc`), pins the managed `Stream` in a `GCHandle`, assigns `AvioWritePacket` callback, creates `AVIOContext` with `avio_alloc_context`, and wires `fmt->pb` plus `AVFMT_FLAG_CUSTOM_IO`; allocation failures are explicitly surfaced via exceptions/cleanup.


#### [[FfEncoder.SetupIo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void SetupIo(AVFormatContext* fmt, Stream output, FfEncoder.EncoderSink sink, ref AVIOContext* customIo, ref GCHandle handle, ref avio_alloc_context_write_packet writeCallback)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]

