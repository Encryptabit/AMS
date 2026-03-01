---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
---
# UndoService::SaveManifest
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.SaveManifest]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void SaveManifest(string chapterStem)
```

**Calls ->**
- [[UndoService.GetManifestPath]]

**Called-by <-**
- [[UndoService.RemoveRecord]]
- [[UndoService.SaveOriginalSegment]]

