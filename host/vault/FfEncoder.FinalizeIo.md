---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::FinalizeIo
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Finalizes FFmpeg IO and transfers any buffered encoded data to the destination stream.**

This method performs sink-specific output finalization after encoding. For `EncoderSink.DynamicBuffer`, it closes FFmpeg’s dynamic buffer (`avio_close_dyn_buf`), validates the returned size with `ThrowIfError`, copies unmanaged bytes into managed memory, writes them to `output`, frees the native buffer, and nulls `fmt->pb`. For non-dynamic sinks, it flushes `customIo` if present.


#### [[FfEncoder.FinalizeIo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void FinalizeIo(AVFormatContext* fmt, Stream output, FfEncoder.EncoderSink sink, ref AVIOContext* customIo)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Complete]]

