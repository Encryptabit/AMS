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
# Native::ams_set_parameter
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Provide the managed P/Invoke-style entry point for setting a native DSP parameter by id with an optional sample offset.**

`ams_set_parameter` is a `public static extern` interop declaration in `Ams.Dsp.Native.Native`, so managed code defines only the call contract while execution is implemented in native code. It passes a parameter id (`uint`), a normalized value (`float value01`), and a sample-frame offset (`uint sampleOffset`) to the native DSP layer. With no managed body, its complexity is effectively constant and it performs no in-method validation or error handling; `SetParameter` uses it as the low-level boundary call.


#### [[Native.ams_set_parameter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_set_parameter(uint id, float value01, uint sampleOffset)
```

**Called-by <-**
- [[AmsDsp.SetParameter]]

