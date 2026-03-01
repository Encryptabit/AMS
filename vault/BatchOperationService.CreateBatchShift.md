---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
---
# BatchOperationService::CreateBatchShift
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs`


#### [[BatchOperationService.CreateBatchShift]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BatchOperationService.CreateBatchShift(System.Collections.Generic.IReadOnlyList{System.String},System.Double)">
    <summary>
    Creates a batch timing shift operation. Shifting means adjusting the gap between
    chapter title reading and content start.
    </summary>
    <param name="chapters">List of chapter stems to shift.</param>
    <param name="shiftSeconds">Shift amount in seconds (positive = later, negative = earlier).</param>
    <returns>The created batch operation.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BatchOperation CreateBatchShift(IReadOnlyList<string> chapters, double shiftSeconds)
```

