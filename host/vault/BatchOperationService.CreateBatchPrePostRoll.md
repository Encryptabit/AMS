---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
---
# BatchOperationService::CreateBatchPrePostRoll
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BatchOperationService.cs`


#### [[BatchOperationService.CreateBatchPrePostRoll]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BatchOperationService.CreateBatchPrePostRoll(System.Collections.Generic.IReadOnlyList{System.String},System.Double,System.Double)">
    <summary>
    Creates a batch pre/post roll standardization operation. When applied, this will
    re-run AudioTreatmentService with standardized TreatmentOptions.
    </summary>
    <param name="chapters">List of chapter stems to standardize.</param>
    <param name="preRollSec">Pre-roll duration in seconds.</param>
    <param name="postRollSec">Post-roll duration in seconds.</param>
    <returns>The created batch operation.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BatchOperation CreateBatchPrePostRoll(IReadOnlyList<string> chapters, double preRollSec, double postRollSec)
```

