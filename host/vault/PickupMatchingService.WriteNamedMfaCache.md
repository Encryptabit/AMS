---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# PickupMatchingService::WriteNamedMfaCache
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.WriteNamedMfaCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void WriteNamedMfaCache(AsrResponse response)
```

**Calls ->**
- [[Log.Debug]]
- [[PickupMatchingService.GetPickupsDir]]

**Called-by <-**
- [[PickupMatchingService.MatchPickupCrxAsync]]

