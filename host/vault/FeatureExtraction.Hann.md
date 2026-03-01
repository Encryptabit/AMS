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
# FeatureExtraction::Hann
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Compute an N-sample Hann window used to weight audio frames before feature extraction.**

`Hann(int n)` allocates a `double[n]` buffer and fills it with a standard Hann taper using `0.5 * (1 - Math.Cos(2 * Math.PI * i / (n - 1)))` for each index. It has an explicit `n == 1` fast path that returns `[1.0]` to avoid division by zero in the denominator and preserve a valid unit window. The method is `private static` and returns the fully populated window array to its caller (`ExtractFeatures`).


#### [[FeatureExtraction.Hann]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double[] Hann(int n)
```

**Called-by <-**
- [[FeatureExtraction.ExtractFeatures]]

