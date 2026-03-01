---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
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
# AudioIntegrityVerifier::SampleSeries
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Sample a numeric frame series at an arbitrary time using safe bounds handling and linear interpolation.**

`SampleSeries` returns a time-sampled value from a frame series with boundary handling and linear interpolation. It returns `MinDb` for empty input, clamps to `series[0]` for `t <= 0`, then computes fractional frame index `idx = t / max(stepSec, 1e-9)` to avoid divide-by-zero. It clamps `i0`/`i1` to valid bounds and interpolates `series[i0] + (series[i1]-series[i0]) * frac` for smooth aligned sampling.


#### [[AudioIntegrityVerifier.SampleSeries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double SampleSeries(double[] series, double stepSec, double t)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

