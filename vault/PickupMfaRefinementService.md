---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 15
dependency_count: 1
pattern: "service"
tags:
  - class
  - pattern/service
---

# PickupMfaRefinementService

> Class in `Ams.Workstation.Server.Services`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`

## Dependencies
- [[BlazorWorkspace]] (`workspace`)

## Properties
- `CacheJsonOptions`: JsonSerializerOptions
- `_workspace`: BlazorWorkspace

## Members
- [[PickupMfaRefinementService..ctor]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]
- [[PickupMfaRefinementService.BuildAlignmentWords]]
- [[PickupMfaRefinementService.NormalizeAlignmentWord]]
- [[PickupMfaRefinementService.AlignMfaWordsToAsrTokens]]
- [[PickupMfaRefinementService.ApplyRefinedTimings]]
- [[PickupMfaRefinementService.IsPlausibleTokenTiming]]
- [[PickupMfaRefinementService.ComputeMfaCacheKey]]
- [[PickupMfaRefinementService.TryReadAsrResponseCache]]
- [[PickupMfaRefinementService.WriteAsrResponseCache]]
- [[PickupMfaRefinementService.FindTextGridFile]]
- [[PickupMfaRefinementService.FindOovListFile]]
- [[PickupMfaRefinementService.LogMfaResult]]
- [[PickupMfaRefinementService.EnsureStagedPickupWav]]
- [[PickupMfaRefinementService.EnsureLabContentAsync]]

