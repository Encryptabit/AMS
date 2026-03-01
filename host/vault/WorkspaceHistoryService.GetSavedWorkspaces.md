---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 2
tags:
  - method
---
# WorkspaceHistoryService::GetSavedWorkspaces
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs`


#### [[WorkspaceHistoryService.GetSavedWorkspaces]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<string> GetSavedWorkspaces(bool existingOnly = false)
```

**Calls ->**
- [[WorkspaceHistoryService.EnsureLoaded]]
- [[WorkspaceHistoryService.Save]]

