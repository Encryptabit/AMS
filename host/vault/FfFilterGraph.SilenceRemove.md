---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# FfFilterGraph::SilenceRemove
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a `silenceremove` filter node from structured silence-trim parameters by converting them to raw FFmpeg arguments.**

This expression-bodied overload formats `SilenceRemoveFilterParams` into a colon-delimited FFmpeg option string and forwards it to `AddRawFilter("silenceremove", ...)`. It emits `start_periods`, `start_threshold`, `stop_periods`, and `stop_threshold` directly from the parameter object without additional clamping or normalization logic. The method returns the same `FfFilterGraph` instance for fluent chaining.


#### [[FfFilterGraph.SilenceRemove]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph SilenceRemove(SilenceRemoveFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

