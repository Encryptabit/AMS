---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 2
fan_in: 5
fan_out: 0
tags:
  - method
---
# PolishService::GetActiveChapterHandleOrThrow
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.GetActiveChapterHandleOrThrow]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterContextHandle GetActiveChapterHandleOrThrow()
```

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.ApplyRoomtoneOperationAsync]]
- [[PolishService.GeneratePreview]]
- [[PolishService.RevertReplacementAsync]]
- [[PolishService.StageReplacement]]

