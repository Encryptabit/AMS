---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 3
tags:
  - method
---
# PickupMatchingService::PairSegmentsToTargets
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.PairSegmentsToTargets]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PickupMatchingService.PairSegmentsToTargets(System.Collections.Generic.List{Ams.Workstation.Server.Models.PickupSegment},System.Collections.Generic.List{Ams.Workstation.Server.Models.CrxPickupTarget})">
    <summary>
    Pairs utterance segments to CRX targets using positional alignment.
    When there are more segments than targets, finds the best starting offset
    by maximizing total confidence.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<PickupMatch> PairSegmentsToTargets(List<PickupSegment> segments, List<CrxPickupTarget> targets)
```

**Calls ->**
- [[LevenshteinMetrics.Similarity_2]]
- [[Log.Warn]]
- [[PickupMatchingService.NormalizeForMatch]]

**Called-by <-**
- [[PickupMatchingService.MatchPickupCrxAsync]]

