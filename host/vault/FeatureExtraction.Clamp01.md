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
---
# FeatureExtraction::Clamp01
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Clamp a numeric score into the normalized probability-like range from 0 to 1.**

`Clamp01(double value)` is an expression-bodied switch expression that bounds a scalar to the closed interval `[0, 1]`. Values below zero map to `0`, values above one map to `1`, and in-range values are returned unchanged. The implementation is branch-minimal and side-effect free, suitable for repeatedly constraining normalized scores in `Detect`.


#### [[FeatureExtraction.Clamp01]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double Clamp01(double value)
```

**Called-by <-**
- [[FeatureExtraction.Detect_2]]

