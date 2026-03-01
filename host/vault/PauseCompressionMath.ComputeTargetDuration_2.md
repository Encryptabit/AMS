---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseCompressionMath::ComputeTargetDuration
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Computes a policy-shaped compressed pause duration relative to class bounds using inside/outside ratios and a soft knee transition.**

`ComputeTargetDuration(duration, bounds, policy)` applies knee-based compression toward the bounds while preserving in-window values. It first returns non-finite durations unchanged, normalizes policy knobs (`knee >= 0`, `ratioInside/ratioOutside >= 1`), and swaps bound endpoints when `min > max`. For durations below `min` or above `max`, it computes inside/outside compressed candidates via `CompressToward`, blends them with `Lerp` based on normalized distance from the bound across `knee`, and clamps to the allowed knee extension (`min - knee` or `max + knee`). Durations already within `[min, max]` pass through unchanged.


#### [[PauseCompressionMath.ComputeTargetDuration_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeTargetDuration(double duration, PauseCompressionMath.PauseBounds bounds, PausePolicy policy)
```

**Calls ->**
- [[PauseCompressionMath.CompressToward]]
- [[PauseCompressionMath.Lerp]]

**Called-by <-**
- [[PauseCompressionMath.ComputeTargetDuration]]

