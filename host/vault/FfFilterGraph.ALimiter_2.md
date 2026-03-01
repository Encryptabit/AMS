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
# FfFilterGraph::ALimiter
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add an audio limiter filter to the graph using direct numeric settings.**

This expression-bodied overload is a convenience wrapper that builds `ALimiterFilterParams` from scalar inputs and delegates to `ALimiter(ALimiterFilterParams?)`. It exposes defaults (`limitDb = -3`, `attack = 5`, `release = 50`) for quick limiter setup while centralizing formatting and FFmpeg option construction in the parameter-object overload.


#### [[FfFilterGraph.ALimiter_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.ALimiter(System.Double,System.Double,System.Double)">
    <summary>
    Safety limiter (libavfilter <c>alimiter</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph ALimiter(double limitDb = -3, double attack = 5, double release = 50)
```

**Calls ->**
- [[FfFilterGraph.ALimiter]]

