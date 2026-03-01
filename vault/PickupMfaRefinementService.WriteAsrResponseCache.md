---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PickupMfaRefinementService::WriteAsrResponseCache
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`


#### [[PickupMfaRefinementService.WriteAsrResponseCache]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void WriteAsrResponseCache(string cachePath, AsrResponse response)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

