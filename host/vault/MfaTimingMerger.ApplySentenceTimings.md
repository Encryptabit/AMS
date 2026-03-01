---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::ApplySentenceTimings
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Derives and applies sentence-level timing spans from available aligned word timings.**

ApplySentenceTimings computes per-sentence timing by aggregating word intervals from `timingMap` across each sentence’s inclusive book range `[BookStartIdx, BookEndIdx]`. For each sentence, it scans the range, tracking earliest `start` and latest `end` among mapped words, ignoring missing indices. When at least one mapped word exists, it calls `s.SetTiming(start, end, end - start)` and increments the update count. It returns the number of sentences that received timings.


#### [[MfaTimingMerger.ApplySentenceTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int ApplySentenceTimings(Dictionary<int, (double start, double end)> timingMap, IEnumerable<SentenceTarget> sentences)
```

**Called-by <-**
- [[MfaTimingMerger.MergeAndApply]]

