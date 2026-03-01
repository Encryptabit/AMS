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
# FfFilterGraph::AStats
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Append an audio statistics filter with caller-provided or default raw FFmpeg options.**

This expression-bodied overload adds the FFmpeg `astats` analysis filter by passing a raw option string to `AddRawFilter("astats", args)`. It supplies default arguments `metadata=1:reset=1`, enabling metadata emission and periodic reset behavior out of the box. The method performs no local parsing or validation of the argument string.


#### [[FfFilterGraph.AStats_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.AStats(System.String)">
    <summary>
    Measurement helper (libavfilter <c>astats</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AStats(string args = "metadata=1:reset=1")
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

