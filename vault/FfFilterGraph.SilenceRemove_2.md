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
# FfFilterGraph::SilenceRemove
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.SilenceRemove_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.SilenceRemove(System.String)">
    <summary>
    Silence trimming (libavfilter <c>silenceremove</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph SilenceRemove(string args = "start_periods=0:start_threshold=-50dB:stop_periods=0:stop_threshold=-50dB")
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

