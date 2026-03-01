---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# PolishService::ComputeCrxFingerprint
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.ComputeCrxFingerprint]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ComputeCrxFingerprint(IReadOnlyList<CrxPickupTarget> targets)
```

**Called-by <-**
- [[PolishService.ImportPickupsCrxAsync]]

