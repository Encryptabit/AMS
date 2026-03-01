---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
base_class: "Ams.Core.Prosody.PauseScopeBase"
interfaces: []
member_count: 2
dependency_count: 1
pattern: ~
tags:
  - class
---

# ChapterPauseMap

> Class in `Ams.Core.Prosody`

**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

**Inherits from**: [[PauseScopeBase]]

## Dependencies
- [[PauseStatsSet]] (`stats`)

## Properties
- `Timeline`: IReadOnlyList<ChapterTimelineElement>
- `Paragraphs`: IReadOnlyList<ParagraphPauseMap>
- `OriginalStart`: double
- `OriginalEnd`: double
- `CurrentStart`: double
- `CurrentEnd`: double
- `_timeline`: IReadOnlyList<ChapterTimelineElement>
- `_paragraphs`: IReadOnlyList<ParagraphPauseMap>

## Members
- [[ChapterPauseMap..ctor]]
- [[ChapterPauseMap.UpdateBounds]]

