---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PauseCompressionMath::Lerp
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Computes an interpolated value between `a` and `b` at fraction `t`.**

`Lerp` performs linear interpolation between two scalar values using `a + (b - a) * t`. It assumes caller-managed bounds for `t` and applies no clamping or validation.


#### [[PauseCompressionMath.Lerp]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double Lerp(double a, double b, double t)
```

**Called-by <-**
- [[PauseCompressionMath.ComputeTargetDuration_2]]

