---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
---
# PauseDynamicsService::ExtractTimesFromBookWords
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.ExtractTimesFromBookWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<double> ExtractTimesFromBookWords(SentenceAlign sentence, BookIndex bookIndex)
```

**Calls ->**
- [[PauseDynamicsService.IsIntraSentencePunctuation]]

**Called-by <-**
- [[PauseDynamicsService.GetPunctuationTimes]]

