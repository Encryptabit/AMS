---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# PickupMatchingService::TryReadNamedAsrCache
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.TryReadNamedAsrCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private AsrResponse TryReadNamedAsrCache(string pickupFilePath)
```

**Calls ->**
- [[PickupMatchingService.GetPickupsDir]]

**Called-by <-**
- [[PickupMatchingService.MatchPickupCrxAsync]]

