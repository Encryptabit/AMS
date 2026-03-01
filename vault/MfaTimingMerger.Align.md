---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 16
fan_in: 1
fan_out: 2
tags:
  - method
  - danger/high-complexity
---
# MfaTimingMerger::Align
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[MfaTimingMerger.Align]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AlignmentResult Align(List<BookTok> book, List<TgTok> tg)
```

**Calls ->**
- [[MfaTimingMerger.Eq]]
- [[MfaTimingMerger.IsWild]]

**Called-by <-**
- [[MfaTimingMerger.MergeAndApply]]

