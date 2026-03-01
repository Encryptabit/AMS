---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
---
# PipelineConcurrencyControl::CreateShared
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`


#### [[PipelineConcurrencyControl.CreateShared]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PipelineConcurrencyControl CreateShared(int maxAsrParallelism, int maxMfaParallelism)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]

