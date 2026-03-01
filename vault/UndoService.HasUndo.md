---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# UndoService::HasUndo
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.HasUndo]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.UndoService.HasUndo(System.String)">
    <summary>
    Checks if an undo record exists for the given replacement ID.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool HasUndo(string replacementId)
```

**Calls ->**
- [[UndoService.GetUndoRecordInternal]]

