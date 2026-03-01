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
# ParagraphCollector::AddPause
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`


#### [[ParagraphCollector.AddPause]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AddPause(int leftSentenceId, PauseInterval interval)
```

**Calls ->**
- [[ParagraphCollector.AddDuration]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

