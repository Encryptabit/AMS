---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::BuildBookTimingMap
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Converts alignment pairs into a merged `BookIdx` timing dictionary using TextGrid interval bounds.**

BuildBookTimingMap materializes per-book-word timing by joining alignment pairs to TextGrid times and coalescing duplicates. It first creates a lookup `tgSeq -> (start,end)` from `tg`, then iterates `ar.Pairs`, reading each pair’s interval and inserting into a dictionary keyed by `BookIdx`. If a book index already exists (e.g., multiple TG tokens aligned to one word), it merges by taking `Math.Min` start and `Math.Max` end. The resulting map provides one consolidated time span per aligned book word.


#### [[MfaTimingMerger.BuildBookTimingMap]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, (double start, double end)> BuildBookTimingMap(AlignmentResult ar, List<TgTok> tg)
```

**Called-by <-**
- [[MfaTimingMerger.MergeAndApply]]

