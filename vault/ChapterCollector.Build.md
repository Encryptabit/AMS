---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterCollector::Build
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`


#### [[ChapterCollector.Build]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterPauseMap Build(IDictionary<int, ParagraphPauseMap> paragraphMaps)
```

**Calls ->**
- [[PauseStatsSet.FromDurations]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

