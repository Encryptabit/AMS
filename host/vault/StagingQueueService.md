---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 13
dependency_count: 1
pattern: "service"
tags:
  - class
  - pattern/service
---

# StagingQueueService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)

## Properties
- `JsonOptions`: JsonSerializerOptions
- `_workspace`: BlazorWorkspace
- `_lock`: object
- `_queue`: Dictionary<string, List<StagedReplacement>>?

## Members
- [[StagingQueueService..ctor]]
- [[StagingQueueService.Stage]]
- [[StagingQueueService.Unstage]]
- [[StagingQueueService.GetQueue]]
- [[StagingQueueService.GetAllQueued]]
- [[StagingQueueService.UpdateStatus]]
- [[StagingQueueService.UpdateBoundaries]]
- [[StagingQueueService.ShiftDownstream]]
- [[StagingQueueService.Clear]]
- [[StagingQueueService.ClearAll]]
- [[StagingQueueService.GetFilePath]]
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

