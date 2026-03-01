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
---
# FeatureExtraction::NextPow2
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Return the next power-of-two size at or above the input length for downstream signal-processing buffers.**

`NextPow2(int n)` computes the smallest power-of-two integer greater than or equal to `n` using iterative bit shifting. It initializes `p = 1` and repeatedly applies `p <<= 1` while `p < n`, then returns `p`. The implementation is allocation-free and branch-light, intended for FFT-friendly sizing in `ExtractFeatures`.


#### [[FeatureExtraction.NextPow2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int NextPow2(int n)
```

**Called-by <-**
- [[FeatureExtraction.ExtractFeatures]]

