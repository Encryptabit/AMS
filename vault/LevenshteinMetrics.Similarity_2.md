---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 5
fan_in: 3
fan_out: 1
tags:
  - method
---
# LevenshteinMetrics::Similarity
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`


#### [[LevenshteinMetrics.Similarity_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double Similarity(string a, string b)
```

**Calls ->**
- [[LevenshteinMetrics.Distance_3]]

**Called-by <-**
- [[PickupMatchingService.MatchSinglePickupAsync]]
- [[PickupMatchingService.PairSegmentsToTargets]]
- [[PolishVerificationService.RevalidateSegmentAsync]]

