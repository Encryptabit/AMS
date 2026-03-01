---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
---
# PipelineService::WaitAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`


#### [[PipelineService.WaitAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task WaitAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
```

**Called-by <-**
- [[PipelineService.EnsureBookIndexAsync]]
- [[PipelineService.RunChapterAsync]]

