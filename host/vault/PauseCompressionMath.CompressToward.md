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
# PauseCompressionMath::CompressToward
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Moves a value toward a target by scaling their difference with a compression ratio.**

`CompressToward` applies a simple affine compression toward `target` by returning `target + ((value - target) / ratio)`. The caller supplies `ratio`; values greater than `1` shrink distance to target, `1` is a no-op, and values below `1` would expand distance (typically prevented by caller-side clamping).


#### [[PauseCompressionMath.CompressToward]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double CompressToward(double value, double target, double ratio)
```

**Called-by <-**
- [[PauseCompressionMath.ComputeTargetDuration_2]]

