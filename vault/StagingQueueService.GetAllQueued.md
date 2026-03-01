---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# StagingQueueService::GetAllQueued
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.GetAllQueued]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.GetAllQueued">
    <summary>
    Returns all items across all chapters with <see cref="F:Ams.Workstation.Server.Models.ReplacementStatus.Staged"/> status.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<StagedReplacement> GetAllQueued()
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]

