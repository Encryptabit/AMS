---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::PinChannels
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Pins managed audio channel buffers and exposes native pointers for FFmpeg interop.**

This method pins each planar channel array in an `AudioBuffer` so unmanaged FFmpeg code can safely read stable memory addresses. It allocates parallel `GCHandle[]` and pointer arrays sized to `buffer.Channels`, validates each channel is non-null, pins with `GCHandle.Alloc(..., Pinned)`, and records `AddrOfPinnedObject()` per channel. It returns both handles and raw pointers for later encode-time pointer arithmetic and eventual unpinning.


#### [[FfEncoder.PinChannels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (GCHandle[] Handles, nint[] Pointers) PinChannels(AudioBuffer buffer)
```

**Called-by <-**
- [[FfEncoder.Encode]]

