---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# StreamingEncoderSink::Complete
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Finalizes streaming audio encoding and guarantees all buffered encoded data is written out once.**

This method closes the streaming encode session in an idempotent way, returning immediately if not initialized or already completed. It flushes pending resampler data (`FlushResampler`), signals encoder end-of-stream with `avcodec_send_frame(..., null)`, drains remaining packets, writes the container trailer, and finalizes custom-stream IO. On success it sets `_completed` so repeated calls become no-ops.


#### [[StreamingEncoderSink.Complete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Complete()
```

**Calls ->**
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.FinalizeIo]]
- [[FfEncoder.FlushResampler]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[StreamingEncoderSink.Dispose]]

