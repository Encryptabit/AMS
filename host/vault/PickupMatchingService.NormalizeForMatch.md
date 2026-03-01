---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "internal"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
---
# PickupMatchingService::NormalizeForMatch
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.NormalizeForMatch]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PickupMatchingService.NormalizeForMatch(System.String)">
    <summary>
    Normalizes text for fuzzy matching: lowercase, collapse whitespace, remove punctuation.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string NormalizeForMatch(string text)
```

**Called-by <-**
- [[PickupMatchingService.MatchSinglePickupAsync]]
- [[PickupMatchingService.PairSegmentsToTargets]]

