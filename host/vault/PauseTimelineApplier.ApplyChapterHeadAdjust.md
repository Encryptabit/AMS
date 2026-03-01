---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseTimelineApplier::ApplyChapterHeadAdjust
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`

## Summary
**Moves a chapter-head sentence to the requested start offset and shifts following sentences to preserve relative spacing.**

`ApplyChapterHeadAdjust` repositions the target chapter-head sentence to a new absolute start based on `adjust.TargetDurationSec`. It verifies the sentence exists in `indexBySentence` and `timeline`, clamps target start to non-negative, computes `delta = targetStart - timing.StartSec`, and no-ops when `|delta| < DurationEpsilon`. When a shift is needed, it rewrites that sentence’s `SentenceTiming` (`StartSec`/`EndSec` shifted by `delta`, metadata preserved) and propagates the same delta to subsequent sentences via `ShiftFollowingSentences`. This keeps downstream sentence ordering temporally consistent after chapter-head alignment.


#### [[PauseTimelineApplier.ApplyChapterHeadAdjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ApplyChapterHeadAdjust(IDictionary<int, SentenceTiming> timeline, IReadOnlyList<int> orderedSentenceIds, IReadOnlyDictionary<int, int> indexBySentence, PauseAdjust adjust)
```

**Calls ->**
- [[PauseTimelineApplier.ShiftFollowingSentences]]

**Called-by <-**
- [[PauseTimelineApplier.Apply]]

