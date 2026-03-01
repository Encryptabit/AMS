---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
---
# PauseCompressionMath::ComputeTargetDuration
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`


#### [[PauseCompressionMath.ComputeTargetDuration]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double ComputeTargetDuration(double duration, PauseClass class, PausePolicy policy, IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> profiles)
```

**Calls ->**
- [[PauseCompressionMath.ComputeTargetDuration_2]]

**Called-by <-**
- [[CompressionState.RebuildPreview]]
- [[PauseDynamicsService.PlanTransforms]]

