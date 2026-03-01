---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# WorkspaceHistoryService::GetFilePath
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs`


#### [[WorkspaceHistoryService.GetFilePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetFilePath()
```

**Calls ->**
- [[AmsAppDataPaths.Resolve]]

**Called-by <-**
- [[WorkspaceHistoryService.EnsureLoaded]]
- [[WorkspaceHistoryService.Save]]

