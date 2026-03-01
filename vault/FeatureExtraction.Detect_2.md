---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 22
fan_in: 1
fan_out: 9
tags:
  - method
  - danger/high-complexity
---
# FeatureExtraction::Detect
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

> [!danger] High Complexity (22)
> Cyclomatic complexity: 22. Consider refactoring into smaller methods.


#### [[FeatureExtraction.Detect_2]]
##### What it does:
<member name="M:Ams.Core.Audio.FeatureExtraction.Detect(System.Single[],System.Int32,System.Double,System.Double,Ams.Core.Audio.FrameBreathDetectorOptions,System.Collections.Generic.IReadOnlyList{Ams.Core.Audio.PhoneSpan},System.Collections.Generic.IReadOnlyList{Ams.Core.Audio.PhoneSpan})">
    <summary>
    Detects breath regions using a monophonic buffer that shares the audio sample rate.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<Region> Detect(float[] monoSamples, int sampleRate, double startSec, double endSec, FrameBreathDetectorOptions options, IReadOnlyList<PhoneSpan> leftPhones = null, IReadOnlyList<PhoneSpan> rightPhones = null)
```

**Calls ->**
- [[AddRegion]]
- [[FeatureExtraction.BuildProtectionMask]]
- [[FeatureExtraction.Clamp01]]
- [[FeatureExtraction.ExtractFeatures]]
- [[FeatureExtraction.Log1p]]
- [[FeatureExtraction.MergeAndFilter]]
- [[FeatureExtraction.Percentile]]
- [[FeatureExtraction.Sigmoid]]
- [[FeatureExtraction.ZNorm]]

**Called-by <-**
- [[FeatureExtraction.Detect]]

