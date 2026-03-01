---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "public"
complexity: 20
fan_in: 3
fan_out: 5
tags:
  - method
  - danger/high-complexity
---
# PauseTimelineApplier::Apply
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.


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

