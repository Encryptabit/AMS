---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterCollector::AbsorbDurations
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Incorporates precomputed pause-duration groups into the chapter collector’s per-class duration store.**

`AbsorbDurations` merges externally provided duration buckets into chapter-level aggregates by iterating each `PauseClass -> List<double>` entry in the input dictionary. For every pair it delegates to `AddDurationRange(kvp.Key, kvp.Value)`, which handles bucket initialization and per-value filtering/append logic. The method itself is a thin orchestration layer with no direct validation or timeline mutation.


#### [[ChapterCollector.AbsorbDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AbsorbDurations(IReadOnlyDictionary<PauseClass, List<double>> durations)
```

**Calls ->**
- [[ChapterCollector.AddDurationRange]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

