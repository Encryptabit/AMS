---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 10
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
  - llm/data-access
---
# FfEncoder::CleanupIo
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Performs final unmanaged IO and handle cleanup for both custom-stream and dynamic-buffer encoder sink modes.**

This cleanup routine releases sink-specific FFmpeg IO resources after encode completion or failure. For `CustomStream`, it frees any allocated AVIO buffer, frees the `AVIOContext`, clears `customIo`, releases the pinned stream `GCHandle`, and calls `GC.KeepAlive(writeCallback)` to preserve delegate lifetime through native teardown. For `DynamicBuffer`, if `fmt->pb` is still open, it closes the dyn buffer, frees returned raw memory if present, and nulls `fmt->pb` to avoid leaks/double-close.


#### [[FfEncoder.CleanupIo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CleanupIo(AVFormatContext* fmt, FfEncoder.EncoderSink sink, ref AVIOContext* customIo, ref GCHandle handle, avio_alloc_context_write_packet writeCallback)
```

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Dispose]]

