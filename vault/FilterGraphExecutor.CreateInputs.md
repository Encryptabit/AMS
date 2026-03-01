---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# FilterGraphExecutor::CreateInputs
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`


#### [[FilterGraphExecutor.CreateInputs]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraphRunner.GraphInputState[] CreateInputs(IReadOnlyList<FfFilterGraphRunner.GraphInput> inputs)
```

**Calls ->**
- [[FilterGraphExecutor.SetupSource]]

**Called-by <-**
- [[FilterGraphExecutor..ctor]]

