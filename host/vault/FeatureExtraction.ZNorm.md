---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FeatureExtraction::ZNorm
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Generate a numerically stable z-normalized copy of a feature vector for downstream detection logic.**

`ZNorm(double[] values)` performs z-score normalization over the input array by computing `mean` via `Average()`, variance via LINQ projection/average of squared deviations, and `std = Math.Sqrt(variance)`. It adds `1e-12` to variance and again to the denominator in `(values[i] - mean) / (std + 1e-12)` to avoid numerical instability or divide-by-zero on near-constant inputs. The method allocates a new `double[]` result and fills it in a single indexed loop, preserving the original input array.


#### [[FeatureExtraction.ZNorm]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double[] ZNorm(double[] values)
```

**Called-by <-**
- [[FeatureExtraction.Detect_2]]

