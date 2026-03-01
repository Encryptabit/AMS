---
namespace: "Ams.Workstation.Server.Models"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Models/PolishModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# StagedReplacementExtensions::ActualReplacedEndSec
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Models/PolishModels.cs`


#### [[StagedReplacementExtensions.ActualReplacedEndSec]]
##### What it does:
<member name="M:Ams.Workstation.Server.Models.StagedReplacementExtensions.ActualReplacedEndSec(Ams.Workstation.Server.Models.StagedReplacement)">
    <summary>
    Where the replacement actually ends in the post-splice audio:
    original start + pickup duration (not the original end).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double ActualReplacedEndSec(StagedReplacement r)
```

**Calls ->**
- [[StagedReplacementExtensions.PickupDuration]]

