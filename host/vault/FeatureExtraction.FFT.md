---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FeatureExtraction::FFT
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Compute an in-place forward or inverse FFT over a complex buffer for spectral-domain feature processing.**

`FFT(Complex[] buffer, bool forward)` implements an in-place radix-2 Cooley-Tukey transform with iterative bit-reversal permutation followed by staged butterfly passes (`len <<= 1`). Each stage computes a principal twiddle factor `wLen = cos(angle) + i*sin(angle)` where `angle = 2π/len * (forward ? -1 : 1)`, then applies butterflies across half-block pairs while updating `w`. The routine supports forward and inverse directions via angle sign, and for inverse mode it normalizes by dividing every output sample by `n`. No additional arrays are allocated; all computation mutates `buffer` directly.


#### [[FeatureExtraction.FFT]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void FFT(Complex[] buffer, bool forward)
```

**Called-by <-**
- [[FeatureExtraction.ExtractFeatures]]

