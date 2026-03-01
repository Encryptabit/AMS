---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::ApplyWordTimings
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Applies mapped word-level timing intervals to hydrate word targets and reports how many were updated.**

ApplyWordTimings iterates `wordTargets` and updates only entries whose nullable `BookIdx` exists in `timingMap`. For each match, it calls `w.SetTiming(start, end, end - start)` using the mapped interval and increments an `updated` counter. Targets without a mapped book index are skipped, and the method returns the number of words successfully hydrated.


#### [[MfaTimingMerger.ApplyWordTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int ApplyWordTimings(Dictionary<int, (double start, double end)> timingMap, IEnumerable<WordTarget> wordTargets)
```

**Called-by <-**
- [[MfaTimingMerger.MergeAndApply]]

