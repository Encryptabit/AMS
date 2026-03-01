---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterCollector::AddDurationRange
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Accumulates a filtered set of pause durations into a chapter-level bucket for one pause class.**

`AddDurationRange` bulk-merges duration samples into the chapter collector’s `_durations` dictionary for a specific `PauseClass`. It lazily initializes the target list when absent, then iterates `durations` and appends only non-negative finite values (`duration >= 0d && double.IsFinite(duration)`). Invalid values are ignored, preserving numeric integrity of aggregated stats input.


#### [[ChapterCollector.AddDurationRange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AddDurationRange(PauseClass pauseClass, IEnumerable<double> durations)
```

**Called-by <-**
- [[ChapterCollector.AbsorbDurations]]

