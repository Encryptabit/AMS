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
  - llm/error-handling
---
# Native::ams_init
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Initialize the native AMS DSP runtime with audio processing parameters and return a status code indicating success or failure.**

ams_init is declared as public static extern on Ams.Dsp.Native.Native, so it is a managed-to-native interop boundary with no C# body. It passes sample_rate, max_block, and channels into the native DSP layer for startup configuration and returns an int status code. With complexity 1 and usage from Create, it is effectively a thin initialization call whose result should be checked by the caller to handle initialization failures.


#### [[Native.ams_init]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern int ams_init(float sample_rate, uint max_block, uint channels)
```

**Called-by <-**
- [[AmsDsp.Create]]

