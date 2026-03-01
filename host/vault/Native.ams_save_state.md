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
  - llm/data-access
---
# Native::ams_save_state
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Expose the native function that serializes DSP state into a caller-provided byte buffer and updates the length by reference.**

ams_save_state is an unsafe extern interop declaration on Ams.Dsp.Native.Native with no managed implementation, so execution is delegated directly to native code. It accepts a raw byte* buffer plus ref nuint inout_len, where length is used as in/out state (input capacity, output written/required size), which SaveState uses to persist DSP state with low marshaling overhead.


#### [[Native.ams_save_state]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_save_state(byte* buf, ref nuint inout_len)
```

**Called-by <-**
- [[AmsDsp.SaveState]]

