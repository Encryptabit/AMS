---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 22
fan_in: 3
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::PlanTransforms
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

> [!danger] High Complexity (22)
> Cyclomatic complexity: 22. Consider refactoring into smaller methods.

## Summary
**Builds a filtered set of pause-duration adjustments from analysis spans using policy bounds, preservation thresholds, and compression math.**

`PlanTransforms` validates `analysis`/`policy`, short-circuits to `PauseTransformSet.Empty` when there are no spans or no computed class profiles (`PauseCompressionMath.BuildProfiles`). It iterates spans, skipping chapter-head crossings, immutable classes (`ChapterHead`, `PostChapterRead`, `Tail`), and intra-sentence spans flagged as non-compressible; for each remaining span it computes a target duration with `ComputeTargetDuration` unless `ShouldPreserve` marks it as top-quantile-preserved. It ignores negligible deltas (`TargetEpsilon`), then emits `PauseAdjust` records with class/sentence IDs and start/end timestamps. Finally it logs counts with `Log.Debug` and returns a `PauseTransformSet` built from the collected adjustments.


#### [[PauseDynamicsService.PlanTransforms]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseTransformSet PlanTransforms(PauseAnalysisReport analysis, PausePolicy policy)
```

**Calls ->**
- [[Log.Debug]]
- [[PauseCompressionMath.BuildProfiles_2]]
- [[PauseCompressionMath.ComputeTargetDuration]]
- [[PauseCompressionMath.ShouldPreserve]]

**Called-by <-**
- [[PauseDynamicsService.Execute]]
- [[PauseDynamicsServiceTests.PlanTransforms_CompressesSentencePauseOutsideWindow]]
- [[PauseDynamicsServiceTests.PlanTransforms_PreservesTopQuantileForLongestGap]]

