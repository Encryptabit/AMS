---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/Native.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/utility
---
# Native::ams_get_latency_samples
**Path**: `Projects/AMS/host/Ams.Dsp.Native/Native.cs`

## Summary
**Expose a native DSP function that returns the current latency measurement in samples to managed C# code.**

`ams_get_latency_samples` is a managed-to-native interop declaration (`public static extern`) in `Ams.Dsp.Native.Native`, so its body is implemented in unmanaged code and resolved at runtime via the class’s P/Invoke metadata. It takes no arguments and returns a `uint`, implying a direct read of a native latency-sample value/counter with no managed branching or state handling (complexity 1).


#### [[Native.ams_get_latency_samples]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern uint ams_get_latency_samples()
```

