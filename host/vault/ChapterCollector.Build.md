---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/data-access
  - llm/utility
  - llm/validation
---
# ChapterCollector::Build
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Builds a chapter-level pause map by merging ordered paragraph maps and chapter pauses into a time-sorted timeline with aggregated pause statistics and timing bounds.**

`Build` assembles chapter output by iterating `_orderedParagraphIds`, resolving each `ParagraphPauseMap` from `paragraphMaps` via `TryGetValue`, and skipping missing entries. For each found paragraph it updates chapter `originalStart/originalEnd`, appends a `ChapterParagraphElement`, and appends any `_pausesByParagraph` items as `ChapterPauseElement`s ordered by `OriginalStart`. It then normalizes unset bounds (`!double.IsFinite`) to `0d`/start, globally sorts the combined timeline by `OriginalStart`, computes stats with `PauseStatsSet.FromDurations(_durations)`, materializes ordered existing paragraphs, and returns a new `ChapterPauseMap`.


#### [[ChapterCollector.Build]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterPauseMap Build(IDictionary<int, ParagraphPauseMap> paragraphMaps)
```

**Calls ->**
- [[PauseStatsSet.FromDurations]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

