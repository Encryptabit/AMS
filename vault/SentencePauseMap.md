---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
base_class: "Ams.Core.Prosody.PauseScopeBase"
interfaces: []
member_count: 2
dependency_count: 2
pattern: ~
tags:
  - class
---

# SentencePauseMap

> Class in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

**Inherits from**: [[PauseScopeBase]]

## Dependencies
- [[SentenceTiming]] (`originalTiming`)
- [[PauseStatsSet]] (`stats`)

## Properties
- `SentenceId`: int
- `ParagraphId`: int
- `OriginalTiming`: SentenceTiming
- `CurrentTiming`: SentenceTiming
- `Timeline`: IReadOnlyList<SentenceTimelineElement>
- `_timeline`: IReadOnlyList<SentenceTimelineElement>

## Members
- [[SentencePauseMap..ctor]]
- [[SentencePauseMap.UpdateTiming]]

