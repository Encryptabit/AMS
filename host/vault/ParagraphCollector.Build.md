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
# ParagraphCollector::Build
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Builds a `ParagraphPauseMap` by merging sentence maps and paragraph-associated pauses into a time-ordered paragraph timeline with aggregated pause statistics and timing bounds.**

`Build` materializes a paragraph-level timeline by iterating `SentenceIds`, looking up each `SentencePauseMap` in `sentenceMaps`, and skipping missing entries. For each resolved sentence it updates paragraph `originalStart/originalEnd` bounds from `sentenceMap.OriginalTiming`, appends a `ParagraphSentenceElement`, then appends any stored `_pausesBySentence` entries as `ParagraphPauseElement`s ordered by `OriginalStart`. It normalizes unset bounds (`+/-Infinity`) to `0`/start, globally orders the combined timeline by `OriginalStart` into a read-only list, computes stats via `PauseStatsSet.FromDurations(_durations)`, and returns a new `ParagraphPauseMap` including filtered ordered sentence maps.


#### [[ParagraphCollector.Build]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ParagraphPauseMap Build(IDictionary<int, SentencePauseMap> sentenceMaps)
```

**Calls ->**
- [[PauseStatsSet.FromDurations]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

