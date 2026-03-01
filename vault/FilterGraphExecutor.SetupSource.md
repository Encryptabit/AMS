---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 3
tags:
  - method
---
# FilterGraphExecutor::SetupSource
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.SetupSource]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraphRunner.GraphInputState SetupSource(string label, AudioBuffer buffer)
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[FfUtils.CloneOrDefault]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FilterGraphExecutor.CreateInputs]]

