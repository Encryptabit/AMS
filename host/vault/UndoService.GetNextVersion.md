---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# UndoService::GetNextVersion
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.GetNextVersion]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private int GetNextVersion(string chapterStem, int sentenceId)
```

**Calls ->**
- [[UndoService.GetChapterUndoDir]]

**Called-by <-**
- [[UndoService.SaveOriginalSegment]]

