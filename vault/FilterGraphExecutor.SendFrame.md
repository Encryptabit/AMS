---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# FilterGraphExecutor::SendFrame
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.SendFrame]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraphRunner.FilterGraphExecutor.SendFrame(Ams.Core.Services.Integrations.FFmpeg.FfFilterGraphRunner.GraphInputState,System.Int32,System.Int32,System.Int64)">
    <summary>
    Sends a frame to the filter graph.
    Returns false if the filter signals EOF (doesn't need more input), true otherwise.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool SendFrame(FfFilterGraphRunner.GraphInputState state, int offset, int sampleCount, long pts)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FilterGraphExecutor.SendAllFrames]]

