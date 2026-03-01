---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::ALimiter
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add an FFmpeg limiter filter node from a parameter object with properly formatted values.**

This overload appends an `alimiter` filter by calling `AddFilter("alimiter", ...)` with serialized `limit`, `attack`, and `release` options. The implementation is null-tolerant (`parameters ?? new ALimiterFilterParams()`), formats dB values with `FormatDecibels`, and formats time values with `FormatDouble` before emission. It returns the current `FfFilterGraph` to support fluent filter-chain composition.


#### [[FfFilterGraph.ALimiter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph ALimiter(ALimiterFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDecibels]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.ALimiter_2]]

