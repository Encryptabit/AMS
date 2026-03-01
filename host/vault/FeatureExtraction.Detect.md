---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FeatureExtraction::Detect
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Prepare and validate buffer-based inputs, then execute breath-region detection on a mono sample stream for the requested time span.**

This overload is a validation/adaptation wrapper that runs breath detection on an `AudioBuffer`. It null-checks `audio`/`options`, early-returns empty for invalid intervals or empty buffers, enforces positive `audio.SampleRate`, and validates that `options.SampleRate` either matches audio or is `0` (infer mode). It derives `effectiveOptions` by filling inferred sample rate when needed, converts/reuses mono samples via `GetOrCreateMonoReference(audio)`, then delegates to the mono core `Detect(float[] mono, int sampleRate, ...)` implementation.


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

