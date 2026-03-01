---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 2
fan_in: 3
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseStatsSet::FromDurations
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Transforms class-indexed raw duration lists into a fully populated per-class pause statistics set with empty defaults for missing buckets.**

`PauseStatsSet.FromDurations` validates its input dictionary and builds a complete `PauseStatsSet` across all supported pause classes. It uses a local `Compute(PauseClass)` helper that checks `durations.TryGetValue(...)` and `list.Count > 0`; when present, it delegates to `PauseStats.FromDurations(list)`, otherwise it returns `PauseStats.Empty`. The method then constructs `PauseStatsSet` in fixed class order (`Comma`, `Sentence`, `Paragraph`, `ChapterHead`, `PostChapterRead`, `Tail`, `Other`), guaranteeing every bucket is populated even if missing in input.


#### [[PauseStatsSet.FromDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseStatsSet FromDurations(IReadOnlyDictionary<PauseClass, List<double>> durations)
```

**Calls ->**
- [[PauseStats.FromDurations]]
- [[Compute]]

**Called-by <-**
- [[ChapterCollector.Build]]
- [[ParagraphCollector.Build]]
- [[SentenceCollector.Build]]

