---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# StagingQueueService::Stage
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.Stage]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.Stage(Ams.Workstation.Server.Models.StagedReplacement)">
    <summary>
    Adds a replacement to the staging queue for its chapter.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Stage(StagedReplacement item)
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

**Called-by <-**
- [[PolishService.StageReplacement]]

