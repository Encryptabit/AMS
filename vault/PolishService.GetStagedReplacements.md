---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# PolishService::GetStagedReplacements
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.GetStagedReplacements]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.GetStagedReplacements(System.String)">
    <summary>
    Returns all staged replacements for a chapter.
    </summary>
    <param name="chapterStem">The chapter stem identifier.</param>
    <returns>Read-only list of staged replacements.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<StagedReplacement> GetStagedReplacements(string chapterStem)
```

**Calls ->**
- [[StagingQueueService.GetQueue]]

