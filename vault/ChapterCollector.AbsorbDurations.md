---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterCollector::AbsorbDurations
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`


#### [[ChapterCollector.AbsorbDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AbsorbDurations(IReadOnlyDictionary<PauseClass, List<double>> durations)
```

**Calls ->**
- [[ChapterCollector.AddDurationRange]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

