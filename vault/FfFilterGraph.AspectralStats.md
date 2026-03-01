---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 3
tags:
  - method
---
# FfFilterGraph::AspectralStats
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.AspectralStats]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.AspectralStats(Ams.Core.Services.Integrations.FFmpeg.AspectralStatsFilterParams)">
    <summary>
    Spectral statistics analyzer (libavfilter <c>aspectralstats</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AspectralStats(AspectralStatsFilterParams parameters = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]
- [[FfFilterGraph.FormatFraction]]

