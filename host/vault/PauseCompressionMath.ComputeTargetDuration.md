---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseCompressionMath::ComputeTargetDuration
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Resolves and applies class-specific pause compression rules to produce a target duration.**

This overload computes a class-aware target pause duration by validating `policy` and `profiles`, then looking up the class profile in `profiles`. If no profile exists for the class, it returns the original `duration` unchanged. When a profile is found, it delegates to the bounds-based core overload `ComputeTargetDuration(duration, profile.Bounds, policy)`.


#### [[PauseCompressionMath.ComputeTargetDuration]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double ComputeTargetDuration(double duration, PauseClass class, PausePolicy policy, IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> profiles)
```

**Calls ->**
- [[PauseCompressionMath.ComputeTargetDuration_2]]

**Called-by <-**
- [[CompressionState.RebuildPreview]]
- [[PauseDynamicsService.PlanTransforms]]

