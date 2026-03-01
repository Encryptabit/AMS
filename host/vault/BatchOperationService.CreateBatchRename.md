---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# BatchOperationService::CreateBatchRename
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs`


#### [[BatchOperationService.CreateBatchRename]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BatchOperationService.CreateBatchRename(System.Collections.Generic.IReadOnlyList{System.String},System.String)">
    <summary>
    Creates a batch rename operation. Records the operation intent (naming pattern)
    for application at a later time. Actual rename logic is deferred to apply time.
    </summary>
    <param name="chapters">List of chapter stems to rename.</param>
    <param name="pattern">Naming pattern, e.g. "Chapter {N:D2}".</param>
    <returns>The created batch operation.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BatchOperation CreateBatchRename(IReadOnlyList<string> chapters, string pattern)
```

