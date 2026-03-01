---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "public"
complexity: 31
fan_in: 1
fan_out: 12
tags:
  - method
  - danger/high-complexity
---
# AudioIntegrityVerifier::Verify
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

> [!danger] High Complexity (31)
> Cyclomatic complexity: 31. Consider refactoring into smaller methods.


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

