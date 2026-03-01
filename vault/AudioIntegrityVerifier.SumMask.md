---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# AudioIntegrityVerifier::SumMask
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`


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

