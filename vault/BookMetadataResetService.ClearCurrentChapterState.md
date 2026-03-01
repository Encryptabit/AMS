---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 1
tags:
  - method
---
# BookMetadataResetService::ClearCurrentChapterState
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs`


#### [[BookMetadataResetService.ClearCurrentChapterState]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ClearCurrentChapterState(string workingDirectory)
```

**Calls ->**
- [[AmsAppDataPaths.Resolve]]

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]

