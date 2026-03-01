---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::BuildSpeechMask
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Generate a frame-wise boolean speech mask by thresholding dB values.**

`BuildSpeechMask` converts a decibel series into a binary speech-activity mask using a fixed threshold comparison. It allocates a `bool[]` with the same length as `db`, then performs a single linear pass setting each frame to `true` when `db[i] >= thr` and `false` otherwise. The method is O(n) and allocation-minimal (one output array).


#### [[AudioIntegrityVerifier.BuildSpeechMask]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool[] BuildSpeechMask(double[] db, double thr)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

