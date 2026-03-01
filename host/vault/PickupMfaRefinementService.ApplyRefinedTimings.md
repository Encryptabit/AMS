---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# PickupMfaRefinementService::ApplyRefinedTimings
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`


#### [[PickupMfaRefinementService.ApplyRefinedTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AsrToken[] ApplyRefinedTimings(IReadOnlyList<AsrToken> originalTokens, IReadOnlyDictionary<int, (double Start, double End)> refinedTimings)
```

**Calls ->**
- [[PickupMfaRefinementService.IsPlausibleTokenTiming]]

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

