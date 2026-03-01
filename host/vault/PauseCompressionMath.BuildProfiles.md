---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseCompressionMath::BuildProfiles
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Creates per-pause-class compression profiles by combining policy bounds with data-driven preserve thresholds.**

`BuildProfiles(durations, policy)` validates inputs, then iterates each class-duration bucket and attempts to resolve policy bounds via `TryGetBounds`. For classes with supported bounds, it computes a preservation threshold using `ComputePreserveThreshold(kvp.Value, policy.PreserveTopQuantile)` and stores `new PauseCompressionProfile(bounds, threshold)` in the result dictionary; unsupported classes are skipped. The returned map therefore contains only classes that can be mapped to concrete compression bounds.


#### [[PauseCompressionMath.BuildProfiles]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> BuildProfiles(Dictionary<PauseClass, List<double>> durations, PausePolicy policy)
```

**Calls ->**
- [[PauseCompressionMath.ComputePreserveThreshold]]
- [[PauseCompressionMath.TryGetBounds]]

**Called-by <-**
- [[CompressionState.RebuildPreview]]
- [[PauseCompressionMath.BuildProfiles_2]]

