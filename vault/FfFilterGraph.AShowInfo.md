---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# FfFilterGraph::AShowInfo
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.AShowInfo]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.AShowInfo(System.String)">
    <summary>
    Emit per-frame debug info (libavfilter <c>ashowinfo</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AShowInfo(string level = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]

