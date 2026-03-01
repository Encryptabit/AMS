---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# StagingQueueService::GetQueue
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.GetQueue]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.GetQueue(System.String)">
    <summary>
    Returns all staged replacements for a specific chapter.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<StagedReplacement> GetQueue(string chapterStem)
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]

**Called-by <-**
- [[PolishService.FindStagedItem]]
- [[PolishService.GetStagedReplacements]]

