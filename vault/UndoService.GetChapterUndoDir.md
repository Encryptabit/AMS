---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 1
tags:
  - method
---
# UndoService::GetChapterUndoDir
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.GetChapterUndoDir]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetChapterUndoDir(string chapterStem)
```

**Calls ->**
- [[UndoService.GetWorkDir]]

**Called-by <-**
- [[UndoService.GetManifestPath]]
- [[UndoService.GetNextVersion]]
- [[UndoService.SaveOriginalSegment]]

