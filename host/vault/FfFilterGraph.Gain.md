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
# FfFilterGraph::Gain
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add an FFmpeg volume filter from a gain parameter object.**

This overload materializes a gain/volume filter clause and appends it via `AddFilter("volume", ("volume", ...))`. It is null-tolerant (`parameters ?? new GainFilterParams()`), serializes the multiplier with `FormatDouble`, and returns the same `FfFilterGraph` to preserve fluent chaining. The method encapsulates conversion from typed parameters to FFmpeg `volume` filter arguments.


#### [[FfFilterGraph.Gain]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph Gain(GainFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.Gain_2]]

