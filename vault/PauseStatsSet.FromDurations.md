---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 2
fan_in: 3
fan_out: 2
tags:
  - method
---
# PauseStatsSet::FromDurations
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`


#### [[PauseStatsSet.FromDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseStatsSet FromDurations(IReadOnlyDictionary<PauseClass, List<double>> durations)
```

**Calls ->**
- [[PauseStats.FromDurations]]
- [[Compute]]

**Called-by <-**
- [[ChapterCollector.Build]]
- [[ParagraphCollector.Build]]
- [[SentenceCollector.Build]]

