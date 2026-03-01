---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# FfFilterGraphRunner::Stream
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FfFilterGraphRunner.Stream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Stream(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec, FfFilterGraphRunner.IAudioFrameSink sink)
```

**Calls ->**
- [[FfFilterGraphRunner.ExecuteInternal]]

**Called-by <-**
- [[FfFilterGraph.StreamToWave]]

