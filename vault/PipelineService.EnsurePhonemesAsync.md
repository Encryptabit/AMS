---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 3
tags:
  - method
---
# PipelineService::EnsurePhonemesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`


#### [[PipelineService.EnsurePhonemesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookIndex> EnsurePhonemesAsync(BookIndex index, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]
- [[PipelineService.CountMissingPhonemes]]

**Called-by <-**
- [[PipelineService.BuildBookIndexAsync]]
- [[PipelineService.BuildBookIndexInternal]]

