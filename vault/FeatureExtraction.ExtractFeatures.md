---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 21
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# FeatureExtraction::ExtractFeatures
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

> [!danger] High Complexity (21)
> Cyclomatic complexity: 21. Consider refactoring into smaller methods.


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

