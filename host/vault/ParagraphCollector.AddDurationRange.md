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
# ParagraphCollector::AddDurationRange
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Appends a filtered set of pause durations to the paragraph collector’s per-class duration aggregate.**

`AddDurationRange` merges multiple duration samples into `_durations[pauseClass]`, lazily creating the target `List<double>` when the class bucket is missing. It iterates the incoming `durations` sequence and appends only values that are non-negative and finite (`duration >= 0d && double.IsFinite(duration)`). Invalid samples are filtered out rather than throwing, preserving robust aggregation behavior.


#### [[ParagraphCollector.AddDurationRange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AddDurationRange(PauseClass pauseClass, IEnumerable<double> durations)
```

**Called-by <-**
- [[ParagraphCollector.AbsorbSentenceDurations]]

