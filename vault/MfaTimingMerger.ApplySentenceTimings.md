---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaTimingMerger::ApplySentenceTimings
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`


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

