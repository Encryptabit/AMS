---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
---
# PauseCompressionMath::ShouldPreserve
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`


#### [[PauseCompressionMath.ShouldPreserve]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool ShouldPreserve(double duration, PauseClass class, IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> profiles)
```

**Called-by <-**
- [[CompressionState.RebuildPreview]]
- [[PauseDynamicsService.PlanTransforms]]

