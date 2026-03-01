---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs"
access_modifier: "private"
complexity: 15
fan_in: 1
fan_out: 5
tags:
  - method
  - danger/high-complexity
---
# CrxService::BuildSeededLegacyEntry
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/CrxService.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[CrxService.BuildSeededLegacyEntry]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private CrxEntry BuildSeededLegacyEntry(CrxEntry excelEntry, DateTime seededAt)
```

**Calls ->**
- [[BlazorWorkspace.TryGetHydratedTranscript]]
- [[CrxService.DistanceToSentenceCenter]]
- [[CrxService.ResolveAudioFileForError]]
- [[CrxService.ResolveWorkspaceChapterName]]
- [[CrxService.TryParseTimecode]]

**Called-by <-**
- [[CrxService.EnsureJsonSeededFromExcel]]

