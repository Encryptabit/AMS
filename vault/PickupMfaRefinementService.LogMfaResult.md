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
# PickupMfaRefinementService::LogMfaResult
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`


#### [[PickupMfaRefinementService.LogMfaResult]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void LogMfaResult(string stage, MfaCommandResult result)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

