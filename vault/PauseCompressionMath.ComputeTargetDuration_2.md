---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
---
# PauseCompressionMath::ComputeTargetDuration
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`


#### [[PauseCompressionMath.ComputeTargetDuration_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeTargetDuration(double duration, PauseCompressionMath.PauseBounds bounds, PausePolicy policy)
```

**Calls ->**
- [[PauseCompressionMath.CompressToward]]
- [[PauseCompressionMath.Lerp]]

**Called-by <-**
- [[PauseCompressionMath.ComputeTargetDuration]]

