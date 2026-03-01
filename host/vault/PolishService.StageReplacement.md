---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
complexity: 11
fan_in: 0
fan_out: 5
tags:
  - method
---
# PolishService::StageReplacement
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.StageReplacement]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.StageReplacement(System.String,Ams.Workstation.Server.Models.PickupMatch,System.String,System.Double,System.Double,System.Double,System.String)">
    <summary>
    Creates a <see cref="T:Ams.Workstation.Server.Models.StagedReplacement"/> from a match and adds it to the staging queue.
    </summary>
    <param name="chapterStem">The chapter stem identifier.</param>
    <param name="match">The pickup match to stage.</param>
    <param name="pickupFilePath">Path to the pickup source file.</param>
    <param name="originalStartSec">Start time of the original sentence in the chapter audio.</param>
    <param name="originalEndSec">End time of the original sentence in the chapter audio.</param>
    <param name="crossfadeSec">Crossfade duration in seconds (default 30ms).</param>
    <param name="curve">Crossfade curve type (default "tri").</param>
    <returns>The created StagedReplacement record.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public StagedReplacement StageReplacement(string chapterStem, PickupMatch match, string pickupFilePath, double originalStartSec, double originalEndSec, double crossfadeSec = 0.03, string curve = "tri")
```

**Calls ->**
- [[SpliceBoundaryService.RefineBoundaries]]
- [[PolishService.GetActiveChapterHandleOrThrow]]
- [[PolishService.GetChapterBuffer]]
- [[PolishService.GetCurrentHydratedTranscript]]
- [[StagingQueueService.Stage]]

