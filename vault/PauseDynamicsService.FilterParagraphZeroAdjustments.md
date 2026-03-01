---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PauseDynamicsService::FilterParagraphZeroAdjustments
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.FilterParagraphZeroAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PauseTransformSet FilterParagraphZeroAdjustments(PauseTransformSet plan, IReadOnlyDictionary<int, int> sentenceToParagraph)
```

**Calls ->**
- [[TouchesParagraphZero]]

**Called-by <-**
- [[PauseDynamicsService.Execute]]

