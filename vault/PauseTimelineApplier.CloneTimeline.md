---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# PauseTimelineApplier::CloneTimeline
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`


#### [[PauseTimelineApplier.CloneTimeline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, SentenceTiming> CloneTimeline(IReadOnlyDictionary<int, SentenceTiming> baseline)
```

**Called-by <-**
- [[PauseTimelineApplier.Apply]]

