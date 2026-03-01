---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::LowerBound
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Return the first position in a start-sorted sentence index where the start time is not less than a target value.**

`LowerBound` implements a standard binary search over a list sorted by `Start` time to find the first index whose `Start >= x`. It maintains a half-open search range (`lo = 0`, `hi = a.Count`) and narrows via midpoint comparisons until convergence. The returned insertion index supports efficient overlap probing in `LookupSentenceContext` without scanning from the beginning.


#### [[AudioIntegrityVerifier.LowerBound]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int LowerBound(List<(double Start, double End, int SentenceId)> a, double x)
```

**Called-by <-**
- [[AudioIntegrityVerifier.LookupSentenceContext]]

