---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# FilterGraphExecutor::BuildOutput
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.BuildOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer BuildOutput()
```

**Calls ->**
- [[AudioAccumulator.ToBuffer]]

**Called-by <-**
- [[FfFilterGraphRunner.ExecuteInternal]]

