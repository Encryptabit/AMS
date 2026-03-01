---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterCollector::AddDuration
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Adds a validated pause duration to the chapter collector’s per-class duration aggregation.**

`AddDuration` maintains chapter-level duration buckets in `_durations` keyed by `PauseClass`, creating a `List<double>` when the class is first encountered (`TryGetValue` fallback). It appends only values that pass validation (`duration >= 0d && double.IsFinite(duration)`), silently discarding invalid samples. This ensures later stats calculation uses only sane numeric durations.


#### [[ChapterCollector.AddDuration]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AddDuration(PauseClass pauseClass, double duration)
```

**Called-by <-**
- [[ChapterCollector.AddPause]]

