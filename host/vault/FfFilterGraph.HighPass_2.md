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
  - llm/validation
---
# FfFilterGraph::HighPass
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a high-pass filter to the `FfFilterGraph` using simple scalar parameters instead of a parameter object.**

This expression-bodied overload is a thin convenience wrapper that instantiates `HighPassFilterParams` from `frequencyHz` (default `70`) and `poles` (default `2`) and delegates to `HighPass(HighPassFilterParams?)`. It preserves fluent API ergonomics while routing behavior through the shared overload where filter argument formatting and pole clamping are handled before emitting the `highpass` filter node.


#### [[FfFilterGraph.HighPass_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.HighPass(System.Double,System.Double)">
    <summary>
    High-pass filter (libavfilter <c>highpass</c>, ffmpeg <c>-af highpass=f=...</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph HighPass(double frequencyHz = 70, double poles = 2)
```

**Calls ->**
- [[FfFilterGraph.HighPass]]

