---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseTimelineApplier::ApplyTailAdjust
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`

## Summary
**Manages the chapter-tail sentinel timing by creating, updating, or removing it based on the requested tail duration.**

`ApplyTailAdjust` applies/removes a synthetic tail segment after a sentence by operating on the mutable `timeline` dictionary keyed by sentence ID. It first validates anchor availability by checking `indexBySentence` and retrieving `leftTiming`; if either lookup fails it exits. It clamps `adjust.TargetDurationSec` to non-negative, removes `TailSentinelSentenceId` when the target tail is effectively zero (`< DurationEpsilon`), otherwise inserts/updates that sentinel entry with a new `SentenceTiming` from `leftTiming.EndSec` to `leftTiming.EndSec + tail`. The `orderedSentenceIds` parameter is unused in this method.


#### [[PauseTimelineApplier.ApplyTailAdjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ApplyTailAdjust(IDictionary<int, SentenceTiming> timeline, IReadOnlyList<int> orderedSentenceIds, IReadOnlyDictionary<int, int> indexBySentence, PauseAdjust adjust)
```

**Called-by <-**
- [[PauseTimelineApplier.Apply]]

