---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseCompressionMath::BuildProfiles
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Builds per-class pause compression profiles from raw pause spans by collecting valid durations and delegating profile computation.**

`BuildProfiles(spans, policy)` validates both inputs, then groups pause durations by `PauseClass` into a `Dictionary<PauseClass, List<double>>` while filtering out non-finite or non-positive span durations. It does not compute profiles directly; instead it delegates to the dictionary-based `BuildProfiles` overload for bounds/threshold derivation. This method is the span-to-duration aggregation front end for pause compression profile generation.


#### [[PauseCompressionMath.BuildProfiles_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> BuildProfiles(IEnumerable<PauseSpan> spans, PausePolicy policy)
```

**Calls ->**
- [[PauseCompressionMath.BuildProfiles]]

**Called-by <-**
- [[PauseDynamicsService.PlanTransforms]]

