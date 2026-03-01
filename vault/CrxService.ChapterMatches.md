---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# CrxService::ChapterMatches
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.ChapterMatches]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ChapterMatches(string left, string right)
```

**Calls ->**
- [[CrxService.NormalizeForCompare]]
- [[CrxService.TryExtractChapterNumber]]

**Called-by <-**
- [[CrxService.ResolveWorkspaceChapterName]]

