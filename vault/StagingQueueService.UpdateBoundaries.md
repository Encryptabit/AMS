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
# StagingQueueService::UpdateBoundaries
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.UpdateBoundaries]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.UpdateBoundaries(System.String,System.Double,System.Double)">
    <summary>
    Updates the original (chapter-side) splice boundaries for a staged replacement.
    Called when the user drags region handles on the waveform.
    Only updates items with <see cref="F:Ams.Workstation.Server.Models.ReplacementStatus.Staged"/> status.
    </summary>
    <param name="replacementId">The replacement to update.</param>
    <param name="newStartSec">New start time in seconds.</param>
    <param name="newEndSec">New end time in seconds.</param>
    <returns>True if the item was found and updated.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool UpdateBoundaries(string replacementId, double newStartSec, double newEndSec)
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

