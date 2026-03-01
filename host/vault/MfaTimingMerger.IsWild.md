---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::IsWild
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Identifies whether a TextGrid alignment token should be treated as a wildcard match.**

IsWild is a sentinel check used during alignment scoring. It returns `true` only when the token equals the merger’s wildcard constant `UNK` (`"unk"`), otherwise `false`.


#### [[MfaTimingMerger.IsWild]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsWild(string t)
```

**Called-by <-**
- [[MfaTimingMerger.Align]]

