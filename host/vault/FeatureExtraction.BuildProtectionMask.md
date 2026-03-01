---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 16
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# FeatureExtraction::BuildProtectionMask
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.

## Summary
**Generate per-frame protection flags that prevent breath detections near window edges and fricative phone boundaries.**

`BuildProtectionMask` creates a frame-aligned boolean suppression mask for candidate breath detection times. It first marks boundary guards near `startSec`/`endSec` using `GuardLeftMs` and `GuardRightMs`, then applies additional guarded windows around fricative-like phones (checked via `IsFricativeLike`) from both `leftPhones` and `rightPhones` using `FricativeGuardMs`. Each qualifying guard interval is projected onto `times` and sets corresponding entries to `true`. The returned `bool[]` is consumed by `Detect` to block detections in protected regions.


#### [[FeatureExtraction.BuildProtectionMask]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool[] BuildProtectionMask(IReadOnlyList<double> times, double startSec, double endSec, FrameBreathDetectorOptions options, IReadOnlyList<PhoneSpan> leftPhones, IReadOnlyList<PhoneSpan> rightPhones)
```

**Calls ->**
- [[FeatureExtraction.IsFricativeLike]]

**Called-by <-**
- [[FeatureExtraction.Detect_2]]

