---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 11
fan_in: 2
fan_out: 2
tags:
  - method
---
# MfaTimingMerger::TokenizeForAlignment
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`


#### [[MfaTimingMerger.TokenizeForAlignment]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> TokenizeForAlignment(string s, bool forTextGrid)
```

**Calls ->**
- [[MfaTimingMerger.SafeNormalize]]
- [[PronunciationHelper.ExtractPronunciationParts]]

**Called-by <-**
- [[MfaTimingMerger.BuildBookTokens]]
- [[MfaTimingMerger.BuildTimedTgTokens]]

