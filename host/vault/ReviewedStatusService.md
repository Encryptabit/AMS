---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs"
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

# ReviewedStatusService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)

## Properties
- `BasePath`: string
- `_workspace`: BlazorWorkspace
- `_status`: Dictionary<string, ReviewedEntry>
- `_currentBookId`: string?

## Members
- [[ReviewedStatusService..ctor]]
- [[ReviewedStatusService.GetAll]]
- [[ReviewedStatusService.IsReviewed]]
- [[ReviewedStatusService.SetReviewed]]
- [[ReviewedStatusService.ResetAll]]
- [[ReviewedStatusService.ResetCurrentBook]]
- [[ReviewedStatusService.EnsureLoaded]]
- [[ReviewedStatusService.GetCurrentBookId]]
- [[ReviewedStatusService.GetFilePath]]
- [[ReviewedStatusService.Load]]
- [[ReviewedStatusService.Save]]

