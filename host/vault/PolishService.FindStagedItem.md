---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 3
fan_in: 3
fan_out: 1
tags:
  - method
---
# PolishService::FindStagedItem
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.FindStagedItem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private StagedReplacement FindStagedItem(string replacementId, string chapterStem)
```

**Calls ->**
- [[StagingQueueService.GetQueue]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.GeneratePreview]]
- [[PolishService.RevertReplacementAsync]]

