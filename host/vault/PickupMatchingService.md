---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 12
dependency_count: 2
pattern: "service"
tags:
  - class
  - pattern/service
---

# PickupMatchingService

> Class in `Ams.Workstation.Server.Services`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)
- [[PickupMfaRefinementService]] (`mfaRefinement`)

## Properties
- `LowConfidenceThreshold`: double
- `MinSegmentDurationSec`: double
- `UtteranceGapSec`: double
- `PunctuationRegex`: Regex
- `WhitespaceRegex`: Regex
- `CacheJsonOptions`: JsonSerializerOptions
- `_workspace`: BlazorWorkspace
- `_mfaRefinement`: PickupMfaRefinementService

## Members
- [[PickupMatchingService..ctor]]
- [[PickupMatchingService.MatchPickupCrxAsync]]
- [[PickupMatchingService.SegmentUtterances]]
- [[PickupMatchingService.PairSegmentsToTargets]]
- [[PickupMatchingService.MatchSinglePickupAsync]]
- [[PickupMatchingService.ExtractFullText]]
- [[PickupMatchingService.NormalizeForMatch]]
- [[PickupMatchingService.BuildAsrOptionsAsync]]
- [[PickupMatchingService.GetPickupsDir]]
- [[PickupMatchingService.TryReadNamedAsrCache]]
- [[PickupMatchingService.WriteNamedAsrCache]]
- [[PickupMatchingService.WriteNamedMfaCache]]

