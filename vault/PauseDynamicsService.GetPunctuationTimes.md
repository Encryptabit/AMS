---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# PauseDynamicsService::GetPunctuationTimes
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.GetPunctuationTimes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<double> GetPunctuationTimes(SentenceAlign sentence, BookIndex bookIndex, HydratedSentence hydratedSentence)
```

**Calls ->**
- [[PauseDynamicsService.ExtractTimesFromBookWords]]
- [[PauseDynamicsService.ExtractTimesFromScript]]

**Called-by <-**
- [[PauseDynamicsService.BuildIntraSentenceSpans]]

