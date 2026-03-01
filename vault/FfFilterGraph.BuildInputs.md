---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
---
# FfFilterGraph::BuildInputs
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.BuildInputs]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<FfFilterGraphRunner.GraphInput> BuildInputs()
```

**Called-by <-**
- [[FfFilterGraph.CaptureLogs]]
- [[FfFilterGraph.RunDiscardingOutput]]
- [[FfFilterGraph.StreamToWave]]
- [[FfFilterGraph.ToBuffer]]

