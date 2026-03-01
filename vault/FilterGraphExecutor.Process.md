---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 4
tags:
  - method
---
# FilterGraphExecutor::Process
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.Process]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Process()
```

**Calls ->**
- [[FilterGraphExecutor.Drain]]
- [[FilterGraphExecutor.SendAllFrames]]
- [[IAudioFrameSink.Complete]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfFilterGraphRunner.ExecuteInternal]]

