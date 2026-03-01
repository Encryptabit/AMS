---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# PickupMfaRefinementService::ComputeMfaCacheKey
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`


#### [[PickupMfaRefinementService.ComputeMfaCacheKey]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ComputeMfaCacheKey(string pickupFilePath, IReadOnlyList<string> alignmentWords)
```

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

