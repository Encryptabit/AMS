---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 7
dependency_count: 2
pattern: "service"
tags:
  - class
  - pattern/service
---

# BatchOperationService

> Class in `Ams.Workstation.Server.Services`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)
- [[StagingQueueService]] (`stagingQueue`)

## Properties
- `_workspace`: BlazorWorkspace
- `_stagingQueue`: StagingQueueService
- `_history`: List<BatchOperation>

## Members
- [[BatchOperationService..ctor]]
- [[BatchOperationService.GetAvailableChapters]]
- [[BatchOperationService.CreateBatchRename]]
- [[BatchOperationService.CreateBatchShift]]
- [[BatchOperationService.CreateBatchPrePostRoll]]
- [[BatchOperationService.CreateBatchDsp]]
- [[BatchOperationService.GetBatchHistory]]

