---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 0
tags:
  - method
---
# PickupMfaRefinementService::IsPlausibleTokenTiming
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`


#### [[PickupMfaRefinementService.IsPlausibleTokenTiming]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsPlausibleTokenTiming(AsrToken original, (double Start, double End) candidate)
```

**Called-by <-**
- [[PickupMfaRefinementService.ApplyRefinedTimings]]

