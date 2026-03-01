---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 4
fan_in: 4
fan_out: 1
tags:
  - method
---
# FfFilterGraph::BuildSpec
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.BuildSpec]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.BuildSpec">
    <summary>
    Build the filter spec string (labels + filter chain).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string BuildSpec()
```

**Calls ->**
- [[FfFilterGraph.EnsureDefaultFormatClause]]

**Called-by <-**
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.RunDiscardingOutput]]
- [[FfFilterGraph.StreamToWave]]
- [[FfFilterGraph.ToBuffer]]

