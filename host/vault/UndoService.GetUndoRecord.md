---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# UndoService::GetUndoRecord
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.GetUndoRecord]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.UndoService.GetUndoRecord(System.String)">
    <summary>
    Returns a single undo record by replacement ID, or null if not found.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public UndoRecord GetUndoRecord(string replacementId)
```

**Called-by <-**
- [[PolishService.RevertReplacementAsync]]

