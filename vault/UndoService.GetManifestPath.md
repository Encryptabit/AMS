---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# UndoService::GetManifestPath
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.GetManifestPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetManifestPath(string chapterStem)
```

**Calls ->**
- [[UndoService.GetChapterUndoDir]]

**Called-by <-**
- [[UndoService.EnsureLoaded]]
- [[UndoService.SaveManifest]]

