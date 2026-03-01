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
  - llm/validation
---
# FfFilterGraph::EbuR128
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add an `ebur128` measurement filter from a parameter object by converting it to raw FFmpeg arguments.**

This expression-bodied overload formats `EbuR128FilterParams` into a raw FFmpeg option string and appends the filter via `AddRawFilter("ebur128", ...)`. Its implementation maps only `FrameLog`, emitting `framelog={parameters.FrameLog}` directly without additional normalization or guarding. It returns the current `FfFilterGraph` for fluent chaining.


#### [[FfFilterGraph.EbuR128]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph EbuR128(EbuR128FilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

