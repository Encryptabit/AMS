---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 0
tags:
  - method
---
# PickupMatchingService::GetPickupsDir
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.GetPickupsDir]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetPickupsDir()
```

**Called-by <-**
- [[PickupMatchingService.TryReadNamedAsrCache]]
- [[PickupMatchingService.WriteNamedAsrCache]]
- [[PickupMatchingService.WriteNamedMfaCache]]

