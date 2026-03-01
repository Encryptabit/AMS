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

# ChapterCollector

> Class in `Ams.Core.Prosody`

**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Properties
- `_orderedParagraphIds`: List<int>
- `_pausesByParagraph`: Dictionary<int, List<PauseInterval>>
- `_durations`: Dictionary<PauseClass, List<double>>

## Members
- [[ChapterCollector..ctor]]
- [[ChapterCollector.AddPause]]
- [[ChapterCollector.AbsorbDurations]]
- [[ChapterCollector.Build]]
- [[ChapterCollector.AddDuration]]
- [[ChapterCollector.AddDurationRange]]

