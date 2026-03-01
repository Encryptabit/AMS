---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 3
tags:
  - method
---
# FilterGraphExecutor::Drain
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.Drain]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void Drain(bool final = false)
```

**Calls ->**
- [[AudioAccumulator.Add]]
- [[IAudioFrameSink.Consume]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FilterGraphExecutor.Process]]
- [[FilterGraphExecutor.SendAllFrames]]

