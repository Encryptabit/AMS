---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 16
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# FeatureExtraction::BuildProtectionMask
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


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

