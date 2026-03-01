---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs"
access_modifier: "public"
complexity: 8
fan_in: 2
fan_out: 2
tags:
  - method
---
# StagingQueueService::ShiftDownstream
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/StagingQueueService.cs`


#### [[StagingQueueService.ShiftDownstream]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.StagingQueueService.ShiftDownstream(System.String,System.Int32,System.Double)">
    <summary>
    Shifts OriginalStartSec and OriginalEndSec for all downstream items in the chapter
    whose SentenceId is greater than <paramref name="pivotSentenceId"/>.
    Applies to both Staged and Applied items so that revert/preview targets the
    correct region even when upstream replacements change duration.
    Call after apply/revert to cascade timing changes to downstream items.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ShiftDownstream(string chapterStem, int pivotSentenceId, double deltaSec)
```

**Calls ->**
- [[StagingQueueService.EnsureLoaded]]
- [[StagingQueueService.Save]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.RevertReplacementAsync]]

