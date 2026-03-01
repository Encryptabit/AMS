---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SentenceCollector::MergeTimeline
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Merges sentence words and detected pause intervals into a single ordered timeline representation.**

`MergeTimeline` combines prebuilt word elements in `_timeline` with pause intervals into a single chronological sequence. It short-circuits when `pauses.Count == 0` by returning a read-only view of `_timeline`; otherwise it preallocates `result`, queues pauses ordered by `OriginalStart`, and iterates words (`_timeline.OfType<SentenceWordElement>()`) ordered by `OriginalStart`, inserting `SentencePauseElement` instances whenever a pause starts before or at the current word’s `OriginalEnd`. After draining remaining pauses, it returns `Array.Empty<SentenceTimelineElement>()` for an empty result or a final `OriginalStart`-sorted read-only list.


#### [[SentenceCollector.MergeTimeline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<SentenceTimelineElement> MergeTimeline(List<PauseInterval> pauses)
```

**Called-by <-**
- [[SentenceCollector.Build]]

