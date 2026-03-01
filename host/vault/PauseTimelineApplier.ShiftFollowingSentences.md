---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseTimelineApplier::ShiftFollowingSentences
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`

## Summary
**Shifts every sentence after a given pivot by a specified time delta to preserve relative ordering after an upstream adjustment.**

`ShiftFollowingSentences` applies a uniform temporal offset to all sentences after a pivot index in `orderedSentenceIds`. It early-returns when `|delta| < DurationEpsilon`, then iterates from `pivotIndex + 1`, looks up each sentence timing in `timeline`, and skips missing entries. For each found sentence, it rewrites `SentenceTiming` with `StartSec + delta` and `EndSec + delta` while preserving `FragmentBacked` and `Confidence`. The `indexBySentence` parameter is unused in this implementation.


#### [[PauseTimelineApplier.ShiftFollowingSentences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ShiftFollowingSentences(IDictionary<int, SentenceTiming> timeline, IReadOnlyList<int> orderedSentenceIds, IReadOnlyDictionary<int, int> indexBySentence, int pivotIndex, double delta)
```

**Called-by <-**
- [[PauseTimelineApplier.Apply]]
- [[PauseTimelineApplier.ApplyChapterHeadAdjust]]
- [[PauseTimelineApplier.ApplyInterSentenceAdjust]]

