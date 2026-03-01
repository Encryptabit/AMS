---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseTimelineApplier::ApplyInterSentenceAdjust
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`

## Summary
**Adjusts the right sentence and all following sentences so the gap after a left sentence matches the requested duration.**

`ApplyInterSentenceAdjust` retimes the right sentence to enforce a target inter-sentence gap after the left sentence. It validates both IDs via `indexBySentence`, ensures `rightIndex > leftIndex`, computes `desiredStart = left.EndSec + targetDuration`, and derives `shift = desiredStart - right.StartSec`; tiny shifts under `DurationEpsilon` are ignored. When shifting, it rewrites the right sentence timing (`newStart = desiredStart`, `newEnd = Math.Max(newStart, right.EndSec + shift)`, preserving metadata) and propagates the same delta to subsequent sentences through `ShiftFollowingSentences`. This keeps timeline continuity while applying gap normalization between sentence boundaries.


#### [[PauseTimelineApplier.ApplyInterSentenceAdjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ApplyInterSentenceAdjust(IDictionary<int, SentenceTiming> timeline, IReadOnlyList<int> orderedSentenceIds, IReadOnlyDictionary<int, int> indexBySentence, int leftId, int rightId, double targetDuration)
```

**Calls ->**
- [[PauseTimelineApplier.ShiftFollowingSentences]]

**Called-by <-**
- [[PauseTimelineApplier.Apply]]

