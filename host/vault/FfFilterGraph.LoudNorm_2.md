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
# FfFilterGraph::LoudNorm
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a loudness-normalization filter to the graph using direct target values.**

This expression-bodied overload is a convenience entry point that constructs `LoudNormFilterParams` from scalar targets and delegates to `LoudNorm(LoudNormFilterParams?)`. It provides defaults (`targetI = -18`, `targetLra = 7`, `targetTp = -2`, `dualMono = true`) so callers can quickly apply common loudness normalization settings, while actual option formatting and filter insertion are centralized in the parameterized overload.


#### [[FfFilterGraph.LoudNorm_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.LoudNorm(System.Double,System.Double,System.Double,System.Boolean)">
    <summary>
    Loudness normalization (libavfilter <c>loudnorm</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph LoudNorm(double targetI = -18, double targetLra = 7, double targetTp = -2, bool dualMono = true)
```

**Calls ->**
- [[FfFilterGraph.LoudNorm]]

