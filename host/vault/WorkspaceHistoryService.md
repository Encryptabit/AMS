---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 5
dependency_count: 0
pattern: "service"
tags:
  - class
  - pattern/service
---

# WorkspaceHistoryService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/WorkspaceHistoryService.cs`

## Properties
- `MaxEntries`: int
- `JsonOptions`: JsonSerializerOptions
- `PathComparer`: StringComparer
- `_lock`: object
- `_entries`: List<string>?

## Members
- [[WorkspaceHistoryService.GetSavedWorkspaces]]
- [[WorkspaceHistoryService.RememberWorkspace]]
- [[WorkspaceHistoryService.EnsureLoaded]]
- [[WorkspaceHistoryService.GetFilePath]]
- [[WorkspaceHistoryService.Save]]

