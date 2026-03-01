---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# FfPacket::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Releases the native packet allocation held by `FfPacket`.**

This dispose method frees the unmanaged `AVPacket` resource when present and makes disposal idempotent. It checks `_pointer`, copies it to a local variable, calls `av_packet_free(&local)`, then sets `_pointer` to `null` to prevent double-free on subsequent calls. The local indirection matches FFmpeg’s pointer-to-pointer free API.


#### [[FfPacket.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

