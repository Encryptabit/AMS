---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
---
# StagingQueueService::ClearAll
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.ClearAll]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.ClearAll">
    <summary>
    Clears all staging queues across all chapters.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ClearAll()
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]

