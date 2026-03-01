---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# PickupMatchingService::WriteNamedAsrCache
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.WriteNamedAsrCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void WriteNamedAsrCache(string pickupFilePath, AsrResponse response)
```

**Calls ->**
- [[Log.Debug]]
- [[PickupMatchingService.GetPickupsDir]]

**Called-by <-**
- [[PickupMatchingService.MatchPickupCrxAsync]]

