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
# FeatureExtraction::Percentile
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Compute a percentile statistic from a numeric array using sorted-order lookup with linear interpolation.**

`Percentile(double[] values, double percentile)` returns an interpolated percentile from an input vector. It short-circuits empty input with `0`, copies and sorts the data (`ToArray` + `Array.Sort`), then maps the percentile to a fractional index `idx = (percentile / 100.0) * (sorted.Length - 1)`. It selects floor/ceiling neighbors and, when non-integer, linearly interpolates with `weight = idx - lower`; exact-index cases return the matched element directly.


#### [[FeatureExtraction.Percentile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double Percentile(double[] values, double percentile)
```

**Called-by <-**
- [[FeatureExtraction.Detect_2]]

