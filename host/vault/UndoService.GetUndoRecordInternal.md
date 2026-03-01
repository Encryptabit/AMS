---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
---
# UndoService::GetUndoRecordInternal
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.GetUndoRecordInternal]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private UndoRecord GetUndoRecordInternal(string replacementId)
```

**Called-by <-**
- [[UndoService.HasUndo]]
- [[UndoService.LoadOriginalSegment]]

