---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 3
fan_out: 4
tags:
  - method
---
# FfFilterGraph::CaptureLogs
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.CaptureLogs]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.CaptureLogs">
    <summary>
    Run the graph while capturing FFmpeg log output (via <see cref="T:Ams.Core.Services.Integrations.FFmpeg.FfLogCapture"/>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<string> CaptureLogs()
```

**Calls ->**
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Execute_2]]
- [[FfLogCapture.Capture]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]
- [[AudioProcessor.DetectSilence]]
- [[FfFilterGraph.Measure]]

