---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseDynamicsService::Apply
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Applies planned pause adjustments to baseline sentence timings and returns the adjusted timeline with intra-sentence gap metadata.**

`Apply` validates `transforms` and `baseline`, then handles two fast paths before timeline mutation: empty baseline returns an empty read-only timeline, and no pause adjustments returns a read-only clone of baseline (`CloneBaseline`) with no intra-sentence gaps. When adjustments exist, it delegates to `PauseTimelineApplier.Apply(baseline, transforms.PauseAdjusts)` and wraps the returned timeline plus detected intra-sentence gaps into `PauseApplyResult` alongside the original transform set.


#### [[PauseDynamicsService.Apply]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseApplyResult Apply(PauseTransformSet transforms, IReadOnlyDictionary<int, SentenceTiming> baseline)
```

**Calls ->**
- [[PauseDynamicsService.CloneBaseline]]
- [[PauseTimelineApplier.Apply]]

**Called-by <-**
- [[PauseDynamicsService.Execute]]

