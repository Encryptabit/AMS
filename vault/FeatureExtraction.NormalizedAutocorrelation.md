---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
---
# FeatureExtraction::NormalizedAutocorrelation
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`


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

