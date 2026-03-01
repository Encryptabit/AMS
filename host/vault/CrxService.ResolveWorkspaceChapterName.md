---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# CrxService::ResolveWorkspaceChapterName
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`


#### [[CrxService.ResolveWorkspaceChapterName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string ResolveWorkspaceChapterName(string chapterLabel)
```

**Calls ->**
- [[CrxService.ChapterMatches]]

**Called-by <-**
- [[CrxService.BuildSeededLegacyEntry]]

