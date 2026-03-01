---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FeatureExtraction::Log1p
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Produce a safe per-element `log(1+x)` style transform of a numeric vector with domain protection for negative inputs.**

`Log1p(double[] values)` allocates a new output array and applies an element-wise guarded log transform. For each input `v`, it returns `0.0` when `v <= -1`; otherwise it computes `Math.Log(1.0 + Math.Max(-0.999, v))`, effectively clamping the lower bound to keep the logarithm domain valid and numerically stable near `-1`. The method is pure with respect to input mutation, returning a transformed copy.


#### [[FeatureExtraction.Log1p]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double[] Log1p(double[] values)
```

**Called-by <-**
- [[FeatureExtraction.Detect_2]]

