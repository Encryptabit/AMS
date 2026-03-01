---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioIntegrityVerifier::Percentile
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Compute a bounded percentile value from a sorted numeric sample set.**

`Percentile` returns a quantile from a sorted `List<double>` with defensive bounds handling. It emits `MinDb` when the list is empty, clamps `p` to `[0,1]`, computes fractional rank `idx = p * (Count - 1)`, and linearly interpolates between adjacent elements (`i0`, `i1`) unless both indices are the same. This yields smooth percentile estimates for threshold inference.


#### [[AudioIntegrityVerifier.Percentile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double Percentile(List<double> sorted, double p)
```

**Called-by <-**
- [[AudioIntegrityVerifier.InferSpeechThreshold]]

