---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# PipelineService::BuildBookIndexInternal
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`


#### [[PipelineService.BuildBookIndexInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<BookIndex> BuildBookIndexInternal(FileInfo bookFile, double averageWpm, CancellationToken cancellationToken)
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]
- [[PipelineService.EnsurePhonemesAsync]]

**Called-by <-**
- [[PipelineService.BuildBookIndexAsync]]

