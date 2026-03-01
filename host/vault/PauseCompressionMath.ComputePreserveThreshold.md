---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseCompressionMath::ComputePreserveThreshold
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Derives a quantile-based preservation threshold from pause durations for top-end pause retention.**

`ComputePreserveThreshold` computes the duration cutoff above which pauses should be preserved, returning `double.PositiveInfinity` when there are too few usable samples (`null`/count<2 or filtered count<2) or when `preserveTopQuantile <= 0`. It filters durations to finite positive values, sorts ascending, clamps quantile to `[0,1]`, and handles edge quantiles (`>=1` returns max sample). For interior quantiles it performs linear interpolation between floor/ceiling rank positions to produce a continuous threshold.


#### [[PauseCompressionMath.ComputePreserveThreshold]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double? ComputePreserveThreshold(IReadOnlyList<double> durations, double preserveTopQuantile)
```

**Called-by <-**
- [[PauseCompressionMath.BuildProfiles]]

