---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 8
dependency_count: 3
pattern: "service"
tags:
  - class
  - pattern/service
---

# PolishVerificationService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)
- [[ReviewedStatusService]] (`reviewedStatus`)
- [[StagingQueueService]] (`stagingQueue`)

## Properties
- `PassThreshold`: double
- `_workspace`: BlazorWorkspace
- `_reviewedStatus`: ReviewedStatusService
- `_stagingQueue`: StagingQueueService
- `_history`: ConcurrentDictionary<string, List<RevalidationResult>>

## Members
- [[PolishVerificationService..ctor]]
- [[PolishVerificationService.RevalidateSegmentAsync]]
- [[PolishVerificationService.SyncToProofAsync]]
- [[PolishVerificationService.GetRevalidationHistory]]
- [[PolishVerificationService.RecordResult]]
- [[PolishVerificationService.ClearHistory]]
- [[PolishVerificationService.ExtractFullText]]
- [[PolishVerificationService.BuildAsrOptionsAsync]]

