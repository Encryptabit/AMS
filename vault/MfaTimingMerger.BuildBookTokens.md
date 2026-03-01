---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaTimingMerger::BuildBookTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`


#### [[MfaTimingMerger.BuildBookTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<BookTok> BuildBookTokens(Func<int, string> getBookToken, int startIdx, int endIdx)
```

**Calls ->**
- [[MfaTimingMerger.TokenizeForAlignment]]

**Called-by <-**
- [[MfaTimingMerger.MergeAndApply]]

