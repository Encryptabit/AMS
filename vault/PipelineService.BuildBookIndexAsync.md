---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 6
tags:
  - method
---
# PipelineService::BuildBookIndexAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`


#### [[PipelineService.BuildBookIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task BuildBookIndexAsync(PipelineRunOptions options, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]
- [[DocumentProcessor.CreateBookCache]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]
- [[PipelineService.BuildBookIndexInternal]]
- [[PipelineService.EnsurePhonemesAsync]]

**Called-by <-**
- [[PipelineService.EnsureBookIndexAsync]]

