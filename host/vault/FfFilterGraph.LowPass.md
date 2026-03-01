---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::LowPass
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add a validated FFmpeg low-pass filter node from a parameter object.**

This overload materializes low-pass filter arguments and appends them via `AddFilter("lowpass", ...)`. The implementation is null-tolerant (`parameters ?? new LowPassFilterParams()`), serializes numeric values with `FormatDouble`, and clamps `p.Poles` to the valid `[1, 2]` range before formatting. It returns the same `FfFilterGraph` for fluent filter-chain composition.


#### [[FfFilterGraph.LowPass]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph LowPass(LowPassFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.LowPass_2]]

