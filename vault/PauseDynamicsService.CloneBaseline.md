---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# PauseDynamicsService::CloneBaseline
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.CloneBaseline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, SentenceTiming> CloneBaseline(IReadOnlyDictionary<int, SentenceTiming> baseline)
```

**Called-by <-**
- [[PauseDynamicsService.Apply]]

