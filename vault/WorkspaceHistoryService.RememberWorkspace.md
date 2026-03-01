---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
---
# WorkspaceHistoryService::RememberWorkspace
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs`


#### [[WorkspaceHistoryService.RememberWorkspace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void RememberWorkspace(string path)
```

**Calls ->**
- [[WorkspaceHistoryService.EnsureLoaded]]
- [[WorkspaceHistoryService.Save]]

