---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::ComputeDbSeries
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Transform PCM samples into a windowed decibel time series for speech-activity and mismatch analysis.**

`ComputeDbSeries` computes a frame-wise RMS energy curve in dB from mono samples using a sliding analysis window. It derives `win` and `step` in samples with lower bounds (`win >= 32`, `step >= 1`), preallocates `frames = max(1, ceil(samples.Length/step))`, and for each frame calculates RMS over `[start, min(start+win, samples.Length))`. Each frame is converted to dB via `20*log10(rms)` with silence clamped to `MinDb` (`-120`), and out-of-range frame starts are also set to `MinDb`. The result is a dense `double[]` time series used downstream for thresholding and alignment.


#### [[AudioIntegrityVerifier.ComputeDbSeries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double[] ComputeDbSeries(float[] samples, int sr, double windowSec, double stepSec)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

