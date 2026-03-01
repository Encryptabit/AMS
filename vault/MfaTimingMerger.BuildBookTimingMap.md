---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaTimingMerger::BuildBookTimingMap
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`


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

