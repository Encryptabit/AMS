---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseCompressionMath::ShouldPreserve
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Checks whether a pause is in the preserved (non-compressed) region for its class based on computed profile thresholds.**

`ShouldPreserve` determines whether a pause duration should bypass compression by consulting the class profile map. It validates `profiles`, returns `false` when the class has no profile or the profile lacks a `PreserveThreshold`, and otherwise returns `duration >= profile.PreserveThreshold.Value`. The method is a pure predicate with no mutation.


#### [[PauseCompressionMath.ShouldPreserve]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool ShouldPreserve(double duration, PauseClass class, IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> profiles)
```

**Called-by <-**
- [[CompressionState.RebuildPreview]]
- [[PauseDynamicsService.PlanTransforms]]

