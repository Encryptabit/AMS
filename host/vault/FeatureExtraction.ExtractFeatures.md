---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 21
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FeatureExtraction::ExtractFeatures
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

> [!danger] High Complexity (21)
> Cyclomatic complexity: 21. Consider refactoring into smaller methods.

## Summary
**Compute frame-level acoustic features (time, energy, spectral, and periodicity cues) used by the breath detector over a target audio interval.**

`ExtractFeatures` slices the requested sample window (`startSec..endSec`), computes frame/hop sizes from options, and returns empty `FrameFeatures` when the window is too short for at least one frame. It optionally applies pre-emphasis, then for each Hann-windowed frame computes time center, RMS dB, and zero-crossing rate, runs an FFT (`FFT`) at `max(FftSize, NextPow2(frame))`, and derives spectral metrics including high/low energy ratio (`HfLf`), high-band spectral flatness, and log-spectrum slope. It also computes normalized autocorrelation (`NormalizedAutocorrelation`) per frame and aggregates all feature arrays into a `FrameFeatures` object. The implementation is frame-loop based with reused working buffers (`fft`, `power`) for efficiency.


#### [[FeatureExtraction.ExtractFeatures]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FrameFeatures ExtractFeatures(float[] samples, int sampleRate, double startSec, double endSec, FrameBreathDetectorOptions options)
```

**Calls ->**
- [[FeatureExtraction.FFT]]
- [[FeatureExtraction.Hann]]
- [[FeatureExtraction.NextPow2]]
- [[FeatureExtraction.NormalizedAutocorrelation]]

**Called-by <-**
- [[FeatureExtraction.Detect_2]]

