---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 1
tags:
  - method
---
# WorkspaceHistoryService::EnsureLoaded
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs`


#### [[WorkspaceHistoryService.EnsureLoaded]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureLoaded()
```

**Calls ->**
- [[WorkspaceHistoryService.GetFilePath]]

**Called-by <-**
- [[WorkspaceHistoryService.GetSavedWorkspaces]]
- [[WorkspaceHistoryService.RememberWorkspace]]

