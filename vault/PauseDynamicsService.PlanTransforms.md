---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 22
fan_in: 3
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# PauseDynamicsService::PlanTransforms
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

> [!danger] High Complexity (22)
> Cyclomatic complexity: 22. Consider refactoring into smaller methods.


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

