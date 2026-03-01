---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 1
tags:
  - method
---
# UndoService::RemoveRecord
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.RemoveRecord]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.UndoService.RemoveRecord(System.String)">
    <summary>
    Deletes the backup file and removes the undo record.
    </summary>
    <returns>True if the record was found and removed.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool RemoveRecord(string replacementId)
```

**Calls ->**
- [[UndoService.SaveManifest]]

