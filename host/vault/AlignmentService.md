---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "Ams.Core.Services.Interfaces.IAlignmentService"
member_count: 4
dependency_count: 4
pattern: "service"
tags:
  - class
  - pattern/service
---

# AlignmentService

> Class in `Ams.Core.Services.Alignment`

**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/AlignmentService.cs`

**Implements**:
- [[IAlignmentService]]

## Dependencies
- [[Ams.Core.Runtime.Book.IPronunciationProvider_]] (`pronunciationProvider`)
- [[Ams.Core.Services.Alignment.IAnchorComputeService_]] (`anchorService`)
- [[Ams.Core.Services.Alignment.ITranscriptIndexService_]] (`transcriptService`)
- [[Ams.Core.Services.Alignment.ITranscriptHydrationService_]] (`hydrationService`)

## Properties
- `_anchorService`: IAnchorComputeService
- `_transcriptService`: ITranscriptIndexService
- `_hydrationService`: ITranscriptHydrationService

## Members
- [[AlignmentService..ctor]]
- [[AlignmentService.ComputeAnchorsAsync]]
- [[AlignmentService.BuildTranscriptIndexAsync]]
- [[AlignmentService.HydrateTranscriptAsync]]

