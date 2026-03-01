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
# FfFilterGraph::DeEsser
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add a de-esser filter node from a parameter object with formatted fractional arguments.**

This overload builds the FFmpeg `deesser` filter invocation and appends it via `AddFilter("deesser", ...)` using short-option keys (`f`, `i`, `m`, `s`). The implementation is null-tolerant (`parameters ?? new DeEsserFilterParams()`), converts normalized numeric inputs with `FormatFraction`, and forwards `OutputMode` as-is. It returns the same `FfFilterGraph` instance to support fluent chaining.


#### [[FfFilterGraph.DeEsser]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph DeEsser(DeEsserFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatFraction]]

**Called-by <-**
- [[FfFilterGraph.DeEsser_2]]

