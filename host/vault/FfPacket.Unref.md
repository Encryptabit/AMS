---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfPacket::Unref
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Clears the current packet contents so the native packet handle can be reused safely.**

`Unref()` releases the packet’s referenced payload while keeping the `AVPacket` container allocated for reuse. It guards against null by checking `_pointer` and then calls `av_packet_unref(_pointer)` only when the native handle exists. This supports decode loops that repeatedly reuse one packet instance without reallocating it.


#### [[FfPacket.Unref]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Unref()
```

**Called-by <-**
- [[FfDecoder.Decode]]

