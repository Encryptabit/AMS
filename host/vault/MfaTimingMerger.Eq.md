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
# MfaTimingMerger::Eq
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Checks whether two normalized alignment tokens are exactly equal.**

Eq is a trivial token comparator used by the alignment routine. It performs direct ordinal string equality via `a == b` and returns the boolean result.


#### [[MfaTimingMerger.Eq]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool Eq(string a, string b)
```

**Called-by <-**
- [[MfaTimingMerger.Align]]

