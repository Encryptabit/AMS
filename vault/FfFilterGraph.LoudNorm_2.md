---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# FfFilterGraph::LoudNorm
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


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

