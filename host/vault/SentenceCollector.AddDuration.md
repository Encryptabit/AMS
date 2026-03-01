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
# SentenceCollector::AddDuration
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Accumulates validated pause durations per pause class into the sentence collector’s duration buckets.**

`AddDuration` updates the `_durations` dictionary keyed by `PauseClass`, lazily allocating a `List<double>` when the class has no entry (`TryGetValue` + insert). It then validates input and appends only durations that are non-negative and finite (`duration >= 0d && double.IsFinite(duration)`). Invalid values are silently ignored, preventing NaN/Infinity/negative samples from polluting pause statistics.


#### [[SentenceCollector.AddDuration]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AddDuration(PauseClass pauseClass, double duration)
```

**Called-by <-**
- [[SentenceCollector.AddPause]]

