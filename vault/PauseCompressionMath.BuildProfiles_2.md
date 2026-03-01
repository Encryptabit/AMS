---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# PauseCompressionMath::BuildProfiles
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`


#### [[PauseCompressionMath.BuildProfiles_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> BuildProfiles(IEnumerable<PauseSpan> spans, PausePolicy policy)
```

**Calls ->**
- [[PauseCompressionMath.BuildProfiles]]

**Called-by <-**
- [[PauseDynamicsService.PlanTransforms]]

