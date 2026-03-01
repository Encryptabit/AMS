---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# FfEncoder::AvioWritePacket
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Implements FFmpeg’s custom AVIO packet writer by forwarding encoded bytes into a managed stream.**

This unmanaged write callback bridges FFmpeg custom IO to a managed `Stream`. It reconstructs a `GCHandle` from `opaque`, casts its target to `Stream`, and writes the native byte span (`buf`, `bufSize`) via `Stream.Write(ReadOnlySpan<byte>)`, returning the byte count on success. Any exception is swallowed and converted to `AVERROR_EOF` so FFmpeg sees a write failure/termination signal.


#### [[FfEncoder.AvioWritePacket]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int AvioWritePacket(void* opaque, byte* buf, int bufSize)
```

