---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 9
fan_in: 0
fan_out: 6
tags:
  - method
---
# FilterGraphExecutor::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FilterGraphExecutor(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs, string filterSpec, FfFilterGraphRunner.FilterExecutionMode mode, FfFilterGraphRunner.IAudioFrameSink frameSink)
```

**Calls ->**
- [[FilterGraphExecutor.ConfigureGraph]]
- [[FilterGraphExecutor.CreateInputs]]
- [[FilterGraphExecutor.RefreshOutputFormat]]
- [[FilterGraphExecutor.SetupSink]]
- [[IAudioFrameSink.Initialize]]
- [[FfSession.EnsureFiltersAvailable]]

