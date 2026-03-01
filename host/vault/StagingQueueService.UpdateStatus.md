---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
---
# StagingQueueService::UpdateStatus
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.UpdateStatus]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.UpdateStatus(System.String,Ams.Workstation.Server.Models.ReplacementStatus)">
    <summary>
    Transitions the status of a replacement (e.g., Staged -> Applied, Applied -> Reverted).
    </summary>
    <returns>True if the item was found and updated.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool UpdateStatus(string replacementId, ReplacementStatus newStatus)
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.RevertReplacementAsync]]

