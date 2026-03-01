---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
base_class: ~
interfaces: []
member_count: 7
dependency_count: 3
pattern: ~
tags:
  - class
---

# SentenceCollector

> Class in `Ams.Core.Prosody`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Dependencies
- [[SentenceAlign]] (`sentence`)
- [[Ams.Core.Artifacts.Hydrate.HydratedSentence_]] (`hydrated`)
- [[BookIndex]] (`bookIndex`)

## Properties
- `SentenceId`: int
- `ParagraphId`: int
- `OriginalTiming`: SentenceTiming
- `Durations`: IReadOnlyDictionary<PauseClass, List<double>>
- `_timeline`: List<SentenceTimelineElement>
- `_durations`: Dictionary<PauseClass, List<double>>
- `_pauseIntervals`: List<PauseInterval>

## Members
- [[SentenceCollector..ctor]]
- [[SentenceCollector.AddPause]]
- [[SentenceCollector.Build]]
- [[SentenceCollector.ResolveTiming]]
- [[SentenceCollector.BuildWordTimeline]]
- [[SentenceCollector.MergeTimeline]]
- [[SentenceCollector.AddDuration]]

