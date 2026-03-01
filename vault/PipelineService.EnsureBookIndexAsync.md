---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 4
tags:
  - method
---
# PipelineService::EnsureBookIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`


#### [[PipelineService.EnsureBookIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<bool> EnsureBookIndexAsync(PipelineRunOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineConcurrencyControl.TryClaimBookIndexForce]]
- [[PipelineService.BuildBookIndexAsync]]
- [[PipelineService.Release]]
- [[PipelineService.WaitAsync]]

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

