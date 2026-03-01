---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
---
# SentenceCollector::Build
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Produces the sentence-level pause map artifact from collected pauses, timeline data, and duration aggregates.**

`Build` finalizes a `SentenceCollector` into a `SentencePauseMap` by ordering accumulated pause intervals by `OriginalStart`, merging them with the prebuilt word timeline through `MergeTimeline`, and computing pause statistics via `PauseStatsSet.FromDurations(_durations)`. It returns a map containing `SentenceId`, `ParagraphId`, original timing, merged timeline elements, and derived stats.


#### [[SentenceCollector.Build]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentencePauseMap Build()
```

**Calls ->**
- [[SentenceCollector.MergeTimeline]]
- [[PauseStatsSet.FromDurations]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

