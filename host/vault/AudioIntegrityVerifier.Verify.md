---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "public"
complexity: 31
fan_in: 1
fan_out: 12
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioIntegrityVerifier::Verify
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

> [!danger] High Complexity (31)
> Cyclomatic complexity: 31. Consider refactoring into smaller methods.

## Summary
**Compare raw vs treated chapter audio on a common timeline to quantify and localize missing or extra speech segments with sentence-level context.**

`Verify` performs frame-level integrity comparison between raw and treated mono waveforms by computing RMS dB series (`ComputeDbSeries`) on sliding windows, inferring per-stream speech thresholds (`InferSpeechThreshold`), and building speech masks. It maps treated timeline frames back to raw time via a sentence-based piecewise-linear transform (`BuildPiecewiseMap` + `MapToRaw`), samples aligned raw energy/speech (`SampleSeries`/`SampleMask`), then detects missing/extra speech masks using both speech-state disagreement and a configurable dB delta gate. It extracts minimum-duration runs (`CollectRuns`), merges nearby same-type runs (`MergeRuns`), computes per-run mean dB deltas, and enriches each mismatch with overlapping treated sentence spans from a prebuilt time index (`BuildSentenceIndex`/`LookupSentenceContext`). A second pass guarantees uncovered treated sentences with sufficient energy asymmetry are emitted as explicit sentence-span mismatches, then totals speech/mismatch durations (`SumMask`) and returns an `AudioVerificationResult` summary.


#### [[AudioIntegrityVerifier.Verify]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioVerificationResult Verify(float[] rawMono, int sampleRateRaw, float[] treatedMono, int sampleRateTreated, IReadOnlyDictionary<int, SentenceTiming> rawTimingsById, IReadOnlyDictionary<int, SentenceTiming> treatedTimingsById, double windowMs = 30, double stepMs = 15, double minMismatchMs = 60, double minGapToMergeMs = 40, double minDeltaDb = 20)
```

**Calls ->**
- [[AudioIntegrityVerifier.BuildPiecewiseMap]]
- [[AudioIntegrityVerifier.BuildSentenceIndex]]
- [[AudioIntegrityVerifier.BuildSpeechMask]]
- [[AudioIntegrityVerifier.CollectRuns]]
- [[AudioIntegrityVerifier.ComputeDbSeries]]
- [[AudioIntegrityVerifier.InferSpeechThreshold]]
- [[AudioIntegrityVerifier.LookupSentenceContext]]
- [[AudioIntegrityVerifier.MapToRaw]]
- [[AudioIntegrityVerifier.MergeRuns]]
- [[AudioIntegrityVerifier.SampleMask]]
- [[AudioIntegrityVerifier.SampleSeries]]
- [[AudioIntegrityVerifier.SumMask]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

