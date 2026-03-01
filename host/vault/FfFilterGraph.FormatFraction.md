---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
---
# FfFilterGraph::FormatFraction
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Converts a `double` into the project’s standardized fraction-formatted string for FFmpeg filter parameters.**

`FormatFraction` is an expression-bodied passthrough that delegates to `FfUtils.FormatFraction(value)` for canonical fraction-style numeric serialization. It contains no local computation, branching, or mutation and serves as a local helper used by multiple filter builders.


#### [[FfFilterGraph.FormatFraction]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatFraction(double value)
```

**Calls ->**
- [[FfUtils.FormatFraction]]

**Called-by <-**
- [[FfFilterGraph.AspectralStats]]
- [[FfFilterGraph.DeEsser]]
- [[FfFilterGraph.DynaudNorm]]

