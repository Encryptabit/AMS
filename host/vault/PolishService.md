---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 18
dependency_count: 5
pattern: "service"
tags:
  - class
  - pattern/service
---

# PolishService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)
- [[StagingQueueService]] (`stagingQueue`)
- [[UndoService]] (`undoService`)
- [[PickupMatchingService]] (`pickupMatching`)
- [[PreviewBufferService]] (`previewBuffer`)

## Properties
- `PickupSlicePaddingSec`: double
- `ArtifactJsonOptions`: JsonSerializerOptions
- `_workspace`: BlazorWorkspace
- `_stagingQueue`: StagingQueueService
- `_undoService`: UndoService
- `_pickupMatching`: PickupMatchingService
- `_previewBuffer`: PreviewBufferService

## Members
- [[PolishService..ctor]]
- [[PolishService.ImportPickupsCrxAsync]]
- [[PolishService.StageReplacement]]
- [[PolishService.GeneratePreview]]
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.RevertReplacementAsync]]
- [[PolishService.GetStagedReplacements]]
- [[PolishService.ApplyRoomtoneOperationAsync]]
- [[PolishService.ComputeCrxFingerprint]]
- [[PolishService.TryReadMatchedArtifacts]]
- [[PolishService.WriteMatchedArtifacts]]
- [[PolishService.GetActiveChapterHandleOrThrow]]
- [[PolishService.FindStagedItem]]
- [[PolishService.GetCurrentHydratedTranscript]]
- [[PolishService.GetChapterBuffer]]
- [[PolishService.TrimPickupForReplacement]]
- [[PolishService.PersistCorrectedBuffer]]
- [[PolishService.ResolveSourceBitDepthOrThrow]]

