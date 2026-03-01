---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 11
dependency_count: 1
pattern: "service"
tags:
  - class
  - pattern/service
---

# IgnoredPatternsService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/IgnoredPatternsService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)

## Properties
- `BasePath`: string
- `_workspace`: BlazorWorkspace
- `_ignoredKeys`: HashSet<string>
- `_currentBookId`: string?

## Members
- [[IgnoredPatternsService..ctor]]
- [[IgnoredPatternsService.GetIgnoredKeys]]
- [[IgnoredPatternsService.IsIgnored]]
- [[IgnoredPatternsService.SetIgnored]]
- [[IgnoredPatternsService.ToggleIgnored]]
- [[IgnoredPatternsService.ResetCurrentBook]]
- [[IgnoredPatternsService.EnsureLoaded]]
- [[IgnoredPatternsService.GetCurrentBookId]]
- [[IgnoredPatternsService.GetFilePath]]
- [[IgnoredPatternsService.Load]]
- [[IgnoredPatternsService.Save]]

