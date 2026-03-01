---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# FilterGraphExecutor::SendAllFrames
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.SendAllFrames]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void SendAllFrames(FfFilterGraphRunner.GraphInputState state)
```

**Calls ->**
- [[FilterGraphExecutor.Drain]]
- [[FilterGraphExecutor.SendFrame]]

**Called-by <-**
- [[FilterGraphExecutor.Process]]

