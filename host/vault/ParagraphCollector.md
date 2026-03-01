---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 6
dependency_count: 0
pattern: ~
tags:
  - class
---

# ParagraphCollector

> Class in `Ams.Core.Prosody`

**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Properties
- `ParagraphId`: int
- `SentenceIds`: IReadOnlyList<int>
- `Durations`: IReadOnlyDictionary<PauseClass, List<double>>
- `_pausesBySentence`: Dictionary<int, List<PauseInterval>>
- `_durations`: Dictionary<PauseClass, List<double>>

## Members
- [[ParagraphCollector..ctor]]
- [[ParagraphCollector.AddPause]]
- [[ParagraphCollector.AbsorbSentenceDurations]]
- [[ParagraphCollector.Build]]
- [[ParagraphCollector.AddDuration]]
- [[ParagraphCollector.AddDurationRange]]

