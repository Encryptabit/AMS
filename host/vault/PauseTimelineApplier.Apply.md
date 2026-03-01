---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "public"
complexity: 20
fan_in: 3
fan_out: 5
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseTimelineApplier::Apply
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.

## Summary
**Applies planned pause adjustments to a baseline sentence timeline, shifting affected sentences and producing both updated timings and intra-sentence gap targets.**

`Apply` validates `baseline` and `adjustments`, short-circuits on empty inputs, and otherwise clones baseline timings into a mutable dictionary before processing. It orders sentence IDs by `StartSec`, builds an index map, filters adjustments to finite/non-negative targets, then applies them in start-time order with class-specific paths: `ChapterHead` via `ApplyChapterHeadAdjust`, `Tail` via `ApplyTailAdjust`, intra-sentence gaps by extending the owning sentence, recording `PauseIntraGap`, and calling `ShiftFollowingSentences`, and inter-sentence gaps via `ApplyInterSentenceAdjust` when both sentence IDs exist. Small deltas are ignored with `DurationEpsilon`, and missing IDs are skipped defensively. It returns a `PauseTimelineApplyResult` containing a read-only adjusted timeline and collected intra-sentence gap mappings.


#### [[PauseTimelineApplier.Apply]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseTimelineApplier.PauseTimelineApplyResult Apply(IReadOnlyDictionary<int, SentenceTiming> baseline, IReadOnlyList<PauseAdjust> adjustments)
```

**Calls ->**
- [[PauseTimelineApplier.ApplyChapterHeadAdjust]]
- [[PauseTimelineApplier.ApplyInterSentenceAdjust]]
- [[PauseTimelineApplier.ApplyTailAdjust]]
- [[PauseTimelineApplier.CloneTimeline]]
- [[PauseTimelineApplier.ShiftFollowingSentences]]

**Called-by <-**
- [[ValidateTimingSession.BuildStaticBufferAdjustments]]
- [[PauseDynamicsService.Apply]]
- [[PauseApplierTests.TimelineApplier_ShiftsSubsequentSentences]]

