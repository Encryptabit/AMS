---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::LoudNorm
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add a loudness-normalization filter node from a parameter object with serialized numeric and boolean options.**

This overload appends a `loudnorm` filter by calling `AddFilter("loudnorm", ...)` and mapping fields to FFmpeg keys `I`, `LRA`, `TP`, and `dual_mono`. The implementation is null-tolerant via `parameters ?? new LoudNormFilterParams()`, formats numeric targets with `FormatDouble`, and converts `DualMono` to `"1"`/`"0"`. It returns the same `FfFilterGraph` instance to preserve fluent chaining.


#### [[FfFilterGraph.LoudNorm]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph LoudNorm(LoudNormFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.LoudNorm_2]]

