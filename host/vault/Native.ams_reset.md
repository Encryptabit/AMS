---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/Native.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/utility
---
# Native::ams_reset
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Expose and invoke the native AMS reset routine from managed C# code.**

`ams_reset` is a `static extern` interop declaration on `Ams.Dsp.Native.Native` that maps directly to the native `ams_reset` symbol. It has no parameters and no return value, so all behavior is implemented in unmanaged code, with no managed-side branching, validation, or error handling in this method. The managed `Reset` caller uses this as the thin boundary to trigger DSP/native reset state.


#### [[Native.ams_reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_reset()
```

**Called-by <-**
- [[AmsDsp.Reset]]

