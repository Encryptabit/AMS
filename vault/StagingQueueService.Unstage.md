---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
---
# StagingQueueService::Unstage
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.Unstage]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.Unstage(System.String)">
    <summary>
    Removes a staged replacement by ID. Only removes items with <see cref="F:Ams.Workstation.Server.Models.ReplacementStatus.Staged"/> status.
    </summary>
    <returns>True if the item was found and removed.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool Unstage(string replacementId)
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

