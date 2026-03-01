---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FeatureExtraction::GetOrCreateMonoReference
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Provide a mono sample view for feature extraction by reusing existing mono data or averaging multichannel planar audio.**

`GetOrCreateMonoReference` normalizes an `AudioBuffer` to a mono sample array with minimal copying. It returns `Array.Empty<float>()` when `audio.Planar` is empty, returns the original channel buffer (`audio.Planar[0]`) when already mono, and otherwise allocates a new `float[length]` and downmixes by averaging all channels using `scale = 1f / audio.Channels`. The multichannel path performs nested channel/sample loops and accumulates scaled samples into the mono output.


#### [[FeatureExtraction.GetOrCreateMonoReference]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static float[] GetOrCreateMonoReference(AudioBuffer audio)
```

**Called-by <-**
- [[FeatureExtraction.Detect]]

