---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/validation
  - llm/error-handling
---
# AmsDsp::Reset
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**It resets the native AMS DSP state after verifying the wrapper is initialized.**

The `AmsDsp.Reset()` method is a thin interop wrapper in `Ams.Dsp.Native.AmsDsp` that executes a fixed two-step sequence: `EnsureInit()` followed by the native call `ams_reset`. With complexity 1, it contains no branching or internal logic beyond enforcing initialization before delegating to the unmanaged reset routine.


#### [[AmsDsp.Reset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Reset()
```

**Calls ->**
- [[AmsDsp.EnsureInit]]
- [[Native.ams_reset]]

