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
# Native::ams_shutdown
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Calls the unmanaged AMS shutdown function to release native DSP-side resources during object cleanup.**

ams_shutdown is a static extern interop declaration on Ams.Dsp.Native.Native, so its logic is implemented in unmanaged code rather than C#. With complexity 1 and no managed control flow, it serves as a thin P/Invoke boundary that is invoked from both Dispose and Finalize to perform native teardown in deterministic and fallback cleanup paths.


#### [[Native.ams_shutdown]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_shutdown()
```

**Called-by <-**
- [[AmsDsp.Dispose]]
- [[AmsDsp.Finalize]]

