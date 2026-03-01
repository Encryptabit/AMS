---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 4
tags:
  - method
---
# FfFilterGraph::StreamToWave
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.StreamToWave]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void StreamToWave(Stream output, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.CreateStreamingSink]]
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Stream]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

