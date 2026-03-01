---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 22
fan_in: 1
fan_out: 9
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FeatureExtraction::Detect
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

> [!danger] High Complexity (22)
> Cyclomatic complexity: 22. Consider refactoring into smaller methods.

## Summary
**Detect likely breath regions in a mono audio interval using weighted frame features, energy-aware scoring, boundary protection, and hysteresis run extraction.**

`Detect(float[] monoSamples, ...)` is the core monophonic breath detector that validates inputs (null checks, empty/invalid window fast-return, sample-rate consistency with `options.SampleRate` inference) and then runs `ExtractFeatures` over the requested interval. It derives an energy gate from `Percentile(features.Db, 20)` plus `AmpMarginDb`/`AbsFloorDb`, z-normalizes multiple cues (`Flat`, `Log1p(HfLf)`, `Zcr`, inverse `Nacf`, `Slope`), combines them with configurable weights, optionally attenuates low-energy frames, and maps to probability-like scores via `Sigmoid((s - ScoreCenter) * 3 * Aggressiveness)`. It applies a protection mask (`BuildProtectionMask`) and hysteresis (`ScoreHigh` to enter, `ScoreLow` or protection to exit) to form candidate runs, converts runs to clamped time regions using half-frame padding, and post-processes with `MergeAndFilter` using merge-gap/min-duration thresholds. The output is a filtered `IReadOnlyList<Region>` of candidate breath spans.


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

