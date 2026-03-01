---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "private"
complexity: 4
fan_in: 7
fan_out: 1
tags:
  - method
---
# StagingQueueService::Save
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void Save()
```

**Calls ->**
- [[StagingQueueService.GetFilePath]]

**Called-by <-**
- [[StagingQueueService.Clear]]
- [[StagingQueueService.ClearAll]]
- [[StagingQueueService.ShiftDownstream]]
- [[StagingQueueService.Stage]]
- [[StagingQueueService.Unstage]]
- [[StagingQueueService.UpdateBoundaries]]
- [[StagingQueueService.UpdateStatus]]

