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
  - llm/error-handling
---
# AudioIntegrityVerifier::SampleMask
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Read a boolean speech-state mask at a given timestamp using rounded, clamped frame indexing.**

`SampleMask` performs nearest-frame sampling on a boolean speech mask at time `t`. It returns `false` for empty masks, otherwise computes frame index `round(t / max(stepSec, 1e-9))`, clamps it to `[0, mask.Length-1]`, and returns `mask[i]`. The method avoids out-of-range access and division-by-zero while providing cheap discrete-time lookup.


#### [[AudioIntegrityVerifier.SampleMask]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool SampleMask(bool[] mask, double stepSec, double t)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

