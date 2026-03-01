---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# PauseDynamicsService::Apply
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.Apply]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseApplyResult Apply(PauseTransformSet transforms, IReadOnlyDictionary<int, SentenceTiming> baseline)
```

**Calls ->**
- [[PauseDynamicsService.CloneBaseline]]
- [[PauseTimelineApplier.Apply]]

**Called-by <-**
- [[PauseDynamicsService.Execute]]

