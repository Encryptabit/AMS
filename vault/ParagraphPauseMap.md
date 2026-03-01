---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
base_class: "Ams.Core.Prosody.PauseScopeBase"
interfaces: []
member_count: 2
dependency_count: 1
pattern: ~
tags:
  - class
---

# ParagraphPauseMap

> Class in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

**Inherits from**: [[PauseScopeBase]]

## Dependencies
- [[PauseStatsSet]] (`stats`)

## Properties
- `ParagraphId`: int
- `Timeline`: IReadOnlyList<ParagraphTimelineElement>
- `Sentences`: IReadOnlyList<SentencePauseMap>
- `OriginalStart`: double
- `OriginalEnd`: double
- `CurrentStart`: double
- `CurrentEnd`: double
- `_timeline`: IReadOnlyList<ParagraphTimelineElement>
- `_sentences`: IReadOnlyList<SentencePauseMap>

## Members
- [[ParagraphPauseMap..ctor]]
- [[ParagraphPauseMap.UpdateBounds]]

