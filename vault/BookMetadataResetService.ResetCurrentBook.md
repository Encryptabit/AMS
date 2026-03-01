---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs"
access_modifier: "public"
complexity: 8
fan_in: 0
fan_out: 9
tags:
  - method
---
# BookMetadataResetService::ResetCurrentBook
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs`


#### [[BookMetadataResetService.ResetCurrentBook]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public MetadataResetResult ResetCurrentBook()
```

**Calls ->**
- [[BookMetadataResetService.ClearCurrentChapterState]]
- [[BookMetadataResetService.ClearPolishDirectory]]
- [[BookMetadataResetService.RemoveBookScopedEntries]]
- [[IgnoredPatternsService.GetIgnoredKeys]]
- [[IgnoredPatternsService.ResetCurrentBook]]
- [[PreviewBufferService.Clear]]
- [[ReviewedStatusService.GetAll]]
- [[ReviewedStatusService.ResetCurrentBook]]
- [[StagingQueueService.ClearAll]]

