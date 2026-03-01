---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 5
dependency_count: 5
pattern: "service"
tags:
  - class
  - pattern/service
---

# BookMetadataResetService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BookMetadataResetService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)
- [[ReviewedStatusService]] (`reviewedStatus`)
- [[IgnoredPatternsService]] (`ignoredPatterns`)
- [[StagingQueueService]] (`stagingQueue`)
- [[PreviewBufferService]] (`previewBuffer`)

## Properties
- `JsonOptions`: JsonSerializerOptions
- `PathComparer`: StringComparer
- `_workspace`: BlazorWorkspace
- `_reviewedStatus`: ReviewedStatusService
- `_ignoredPatterns`: IgnoredPatternsService
- `_stagingQueue`: StagingQueueService
- `_previewBuffer`: PreviewBufferService

## Members
- [[BookMetadataResetService..ctor]]
- [[BookMetadataResetService.ResetCurrentBook]]
- [[BookMetadataResetService.RemoveBookScopedEntries]]
- [[BookMetadataResetService.ClearPolishDirectory]]
- [[BookMetadataResetService.ClearCurrentChapterState]]

