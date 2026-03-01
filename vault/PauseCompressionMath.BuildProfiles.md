---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 2
tags:
  - method
---
# PauseCompressionMath::BuildProfiles
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`


#### [[PauseCompressionMath.BuildProfiles]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyDictionary<PauseClass, PauseCompressionMath.PauseCompressionProfile> BuildProfiles(Dictionary<PauseClass, List<double>> durations, PausePolicy policy)
```

**Calls ->**
- [[PauseCompressionMath.ComputePreserveThreshold]]
- [[PauseCompressionMath.TryGetBounds]]

**Called-by <-**
- [[CompressionState.RebuildPreview]]
- [[PauseCompressionMath.BuildProfiles_2]]

