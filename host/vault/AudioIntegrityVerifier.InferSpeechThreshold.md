---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioIntegrityVerifier::InferSpeechThreshold
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Compute an adaptive speech detection threshold from frame-level dB values using filtered percentile statistics.**

`InferSpeechThreshold` derives a speech/noise cutoff from a dB frame series using a robust percentile heuristic. It filters out near-floor values (`db[i] > MinDb + 1`) into a list, returns a fallback threshold of `MinDb + 6` when no usable frames exist, otherwise sorts the retained values and returns the 30th percentile via `Percentile(vals, 0.30)`. This provides an adaptive, low-cost threshold tuned to each signal’s level distribution.


#### [[AudioIntegrityVerifier.InferSpeechThreshold]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double InferSpeechThreshold(double[] db)
```

**Calls ->**
- [[AudioIntegrityVerifier.Percentile]]

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

