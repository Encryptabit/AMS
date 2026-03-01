---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "public"
base_class: ~
interfaces: []
member_count: 14
dependency_count: 7
pattern: "service"
tags:
  - class
  - pattern/service
---

# PipelineService

> Class in `Ams.Core.Services`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`

## Dependencies
- [[GenerateTranscriptCommand]] (`generateTranscript`)
- [[ComputeAnchorsCommand]] (`computeAnchors`)
- [[BuildTranscriptIndexCommand]] (`buildTranscriptIndex`)
- [[HydrateTranscriptCommand]] (`hydrateTranscript`)
- [[RunMfaCommand]] (`runMfa`)
- [[MergeTimingsCommand]] (`mergeTimings`)
- [[Ams.Core.Runtime.Book.IPronunciationProvider_]] (`pronunciationProvider`)

## Properties
- `_generateTranscript`: GenerateTranscriptCommand
- `_computeAnchors`: ComputeAnchorsCommand
- `_buildTranscriptIndex`: BuildTranscriptIndexCommand
- `_hydrateTranscript`: HydrateTranscriptCommand
- `_runMfa`: RunMfaCommand
- `_mergeTimings`: MergeTimingsCommand
- `_pronunciationProvider`: IPronunciationProvider

## Members
- [[PipelineService..ctor]]
- [[PipelineService.RunChapterAsync]]
- [[PipelineService.IsStageEnabled]]
- [[PipelineService.BuildDefaultAnchorOptions]]
- [[PipelineService.EnsureBookIndexAsync]]
- [[PipelineService.BuildBookIndexAsync]]
- [[PipelineService.BuildBookIndexInternal]]
- [[PipelineService.EnsurePhonemesAsync]]
- [[PipelineService.CountMissingPhonemes]]
- [[PipelineService.ResolveTextGridFile]]
- [[PipelineService.CopyTreatedAudio]]
- [[PipelineService.WaitAsync]]
- [[PipelineService.Release]]
- [[PipelineService.ValidateOptions]]

