---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 3
tags:
  - method
---
# FfFilterGraph::DynaudNorm
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.DynaudNorm]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.DynaudNorm(Ams.Core.Services.Integrations.FFmpeg.DynaudNormFilterParams)">
    <summary>
    Dynamic audio normalization (libavfilter <c>dynaudnorm</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph DynaudNorm(DynaudNormFilterParams parameters = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter_2]]
- [[FfFilterGraph.FormatDouble]]
- [[FfFilterGraph.FormatFraction]]

