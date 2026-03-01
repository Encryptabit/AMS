---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
---
# StagingQueueService::Clear
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.Clear]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.Clear(System.String)">
    <summary>
    Clears all staged items for a specific chapter.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Clear(string chapterStem)
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

