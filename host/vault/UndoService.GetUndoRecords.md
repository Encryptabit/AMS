---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# UndoService::GetUndoRecords
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.GetUndoRecords]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.UndoService.GetUndoRecords(System.String)">
    <summary>
    Returns all undo records for a chapter.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<UndoRecord> GetUndoRecords(string chapterStem)
```

**Calls ->**
- [[UndoService.EnsureLoaded]]

