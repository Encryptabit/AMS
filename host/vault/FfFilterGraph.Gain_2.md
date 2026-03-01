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
  - llm/factory
---
# FfFilterGraph::Gain
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a volume/gain adjustment filter to the graph using a scalar multiplier input.**

This expression-bodied overload is a convenience wrapper that creates `GainFilterParams` from `multiplier` (default `1.0`) and delegates to `Gain(GainFilterParams?)`. It keeps fluent call sites minimal while centralizing formatting and filter insertion behavior in the parameter-object overload.


#### [[FfFilterGraph.Gain_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph Gain(double multiplier = 1)
```

**Calls ->**
- [[FfFilterGraph.Gain]]

