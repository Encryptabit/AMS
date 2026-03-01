---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FeatureExtraction::NormalizedAutocorrelation
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Estimate periodicity strength of an audio segment by finding the strongest normalized autocorrelation value across a constrained lag range.**

`NormalizedAutocorrelation(float[] source, int start, int length, int sampleRate)` computes the maximum mean-centered normalized autocorrelation over a lag window derived from pitch-like bounds (`lagMin = max(1, sampleRate/400)`, `lagMax = min(length-1, sampleRate/80)`). It validates the usable range (`lagMin >= lagMax` or `length <= 1`) and returns `0.0` early when no valid comparison exists. For each lag, it accumulates cross-correlation and per-series energies (`e0`, `e1`) with `1e-12` floors to avoid divide-by-zero, then scores `numerator / sqrt(e0*e1)`. It returns the best lag score clamped to `[0,1]`.


#### [[FeatureExtraction.NormalizedAutocorrelation]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double NormalizedAutocorrelation(float[] source, int start, int length, int sampleRate)
```

**Called-by <-**
- [[FeatureExtraction.ExtractFeatures]]

