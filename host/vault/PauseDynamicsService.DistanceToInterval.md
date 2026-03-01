---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PauseDynamicsService::DistanceToInterval
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Measures how far a scalar point is from an interval, with zero cost when the point lies within it.**

`DistanceToInterval` computes a one-dimensional distance from `value` to the closed interval `[start, end]`. It returns `start - value` when `value` is left of the interval, `value - end` when right of it, and `0` when inside the bounds. This yields a non-negative piecewise-linear penalty used by gap matching logic.


#### [[PauseDynamicsService.DistanceToInterval]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double DistanceToInterval(double value, double start, double end)
```

**Called-by <-**
- [[PauseDynamicsService.MatchSilencesToPunctuation]]

