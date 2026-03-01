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
---
# AudioIntegrityVerifier::SumMask
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Compute approximate speech duration from a boolean frame mask using frame step accumulation plus window-tail compensation.**

`SumMask` estimates total active duration from a frame-level boolean mask by accumulating `stepSec` for each `true` frame in a single pass. After summation, it unconditionally adds `windowSec` to account for the trailing analysis-window tail. The method returns a coarse duration estimate aligned with the verifier’s windowed framing model.


#### [[AudioIntegrityVerifier.SumMask]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double SumMask(bool[] mask, double stepSec, double windowSec)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

