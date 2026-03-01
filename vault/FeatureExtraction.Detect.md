---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
---
# FeatureExtraction::Detect
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`


#### [[FeatureExtraction.Detect]]
##### What it does:
<member name="M:Ams.Core.Audio.FeatureExtraction.Detect(Ams.Core.Artifacts.AudioBuffer,System.Double,System.Double,Ams.Core.Audio.FrameBreathDetectorOptions,System.Collections.Generic.IReadOnlyList{Ams.Core.Audio.PhoneSpan},System.Collections.Generic.IReadOnlyList{Ams.Core.Audio.PhoneSpan})">
    <summary>
    Detects breath regions inside a gap using an <see cref="T:Ams.Core.Artifacts.AudioBuffer"/> as the source.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<Region> Detect(AudioBuffer audio, double startSec, double endSec, FrameBreathDetectorOptions options, IReadOnlyList<PhoneSpan> leftPhones = null, IReadOnlyList<PhoneSpan> rightPhones = null)
```

**Calls ->**
- [[FeatureExtraction.Detect_2]]
- [[FeatureExtraction.GetOrCreateMonoReference]]

**Called-by <-**
- [[ValidateCommand.IsBreathSafe]]

