---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 1
fan_in: 5
fan_out: 2
tags:
  - method
---
# FfFilterGraphRunner::ExecuteInternal
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FfFilterGraphRunner.ExecuteInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer ExecuteInternal(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec, FfFilterGraphRunner.FilterExecutionMode mode, FfFilterGraphRunner.IAudioFrameSink frameSink)
```

**Calls ->**
- [[FilterGraphExecutor.BuildOutput]]
- [[FilterGraphExecutor.Process]]

**Called-by <-**
- [[FfFilterGraphRunner.Apply]]
- [[FfFilterGraphRunner.Apply_2]]
- [[FfFilterGraphRunner.Execute]]
- [[FfFilterGraphRunner.Execute_2]]
- [[FfFilterGraphRunner.Stream]]

