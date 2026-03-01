---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
---
# StagingQueueService::GetFilePath
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.GetFilePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetFilePath()
```

**Called-by <-**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

