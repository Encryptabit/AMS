---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
  - llm/data-access
---
# StreamingEncoderSink::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Performs deterministic shutdown of the streaming encoder sink and releases all unmanaged encoder/IO state.**

`Dispose()` finalizes encoding and frees all native FFmpeg resources held by the streaming sink. It calls `Complete()` first to flush/close output, then conditionally frees frame, resampler, codec context (including channel-layout uninit), and format context; during format teardown it invokes `CleanupIo` for custom-stream AVIO resources/handles. Each freed field is reset to null to prevent double-release on subsequent disposal attempts.


#### [[StreamingEncoderSink.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Calls ->**
- [[FfEncoder.CleanupIo]]
- [[StreamingEncoderSink.Complete]]

