---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# FfFilterGraph::RunDiscardingOutput
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.RunDiscardingOutput]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.RunDiscardingOutput">
    <summary>
    Execute the graph in discard-output mode (useful for measurement filters).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void RunDiscardingOutput()
```

**Calls ->**
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Execute_2]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

