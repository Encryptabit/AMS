---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/Native.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/utility
---
# Native::ams_process
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Invoke the native AMS DSP routine to process a specified number of audio frames from input buffer pointers into output buffer pointers.**

ams_process is an unmanaged interop boundary (`static extern`) with no managed body, so execution is delegated directly to a native DSP implementation. Its signature uses raw double pointers (`float**`) for input/output channel buffers plus a frame count (`uint nframes`), indicating low-level block audio processing from unsafe memory. In this codebase it is invoked by ProcessBlock and ProcessLong as the core native processing call.


#### [[Native.ams_process]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_process(float** in_ptrs, float** out_ptrs, uint nframes)
```

**Called-by <-**
- [[AmsDsp.ProcessBlock]]
- [[AmsDsp.ProcessLong]]

