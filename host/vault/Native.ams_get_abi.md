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
  - llm/utility
  - llm/factory
  - llm/validation
---
# Native::ams_get_abi
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Expose a native ABI version query to managed code so creation/initialization code can verify compatibility.**

`ams_get_abi` is a static `extern` interop declaration in `Ams.Dsp.Native.Native`, so its logic is implemented in unmanaged code rather than in C#. The method returns the native DSP ABI version through `out int major` and `out int minor`, and its single caller (`Create`) likely uses those values to gate initialization against ABI compatibility.


#### [[Native.ams_get_abi]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_get_abi(out int major, out int minor)
```

**Called-by <-**
- [[AmsDsp.Create]]

