---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
---
# PauseCompressionMath::ComputePreserveThreshold
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`


#### [[PauseCompressionMath.ComputePreserveThreshold]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double? ComputePreserveThreshold(IReadOnlyList<double> durations, double preserveTopQuantile)
```

**Called-by <-**
- [[PauseCompressionMath.BuildProfiles]]

