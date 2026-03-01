---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
---
# PauseDynamicsService::ExtractTimesFromScript
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.ExtractTimesFromScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<double> ExtractTimesFromScript(int sentenceId, double sentenceStart, double duration, string scriptText, string sourceLabel)
```

**Calls ->**
- [[Log.Debug]]
- [[PauseDynamicsService.IsIntraSentencePunctuation]]

**Called-by <-**
- [[PauseDynamicsService.GetPunctuationTimes]]

