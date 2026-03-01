---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "private"
complexity: 5
fan_in: 9
fan_out: 1
tags:
  - method
---
# StagingQueueService::EnsureLoaded
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.EnsureLoaded]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureLoaded()
```

**Calls ->**
- [[StagingQueueService.GetFilePath]]

**Called-by <-**
- [[StagingQueueService.Clear]]
- [[StagingQueueService.ClearAll]]
- [[StagingQueueService.GetAllQueued]]
- [[StagingQueueService.GetQueue]]
- [[StagingQueueService.ShiftDownstream]]
- [[StagingQueueService.Stage]]
- [[StagingQueueService.Unstage]]
- [[StagingQueueService.UpdateBoundaries]]
- [[StagingQueueService.UpdateStatus]]

