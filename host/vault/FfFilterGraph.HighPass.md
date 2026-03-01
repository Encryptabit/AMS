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
# FfFilterGraph::HighPass
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Build and add a validated FFmpeg high-pass filter node from a parameter object.**

This overload appends a `highpass` filter to the graph by calling `AddFilter("highpass", ...)` with serialized `frequency` and `poles` arguments. In the implementation, `parameters` is null-tolerant (`parameters ?? new HighPassFilterParams()`), both values are converted with `FormatDouble`, and `poles` is constrained via `Math.Clamp(..., 1, 2)` before emission. It returns the `FfFilterGraph` instance to preserve fluent chaining.


#### [[FfFilterGraph.HighPass]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph HighPass(HighPassFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.HighPass_2]]

