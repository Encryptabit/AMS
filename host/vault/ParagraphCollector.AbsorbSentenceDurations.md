---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ParagraphCollector::AbsorbSentenceDurations
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Accumulates all per-sentence pause duration buckets into paragraph-level duration aggregates grouped by pause class.**

`AbsorbSentenceDurations` performs a two-level aggregation pass over `sentenceCollectors`, then over each collector’s `Durations` dictionary (`PauseClass` -> duration list). For each bucket it calls `AddDurationRange(kvp.Key, kvp.Value)`, which merges values into the paragraph-level `_durations` store with lazy list creation and filtering of invalid values. The method only updates duration statistics and does not modify paragraph pause placement or timeline structures.


#### [[ParagraphCollector.AbsorbSentenceDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AbsorbSentenceDurations(IEnumerable<PauseMapBuilder.SentenceCollector> sentenceCollectors)
```

**Calls ->**
- [[ParagraphCollector.AddDurationRange]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

