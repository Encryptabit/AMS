---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 16
fan_in: 1
fan_out: 2
tags:
  - method
  - danger/high-complexity
  - llm/data-access
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::Align
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.

## Summary
**Computes a wildcard-aware global alignment between book tokens and timed TextGrid tokens and returns matched index pairs with alignment metrics.**

Align performs global sequence alignment between book and TextGrid tokens using Needleman–Wunsch dynamic programming with scores `MATCH=2`, `WILD=2` (when TG token is `UNK`), `MISM=-2`, and `GAP=-1`. It fills `dp` and backtrace (`bt`) matrices over `(n+1) x (m+1)` dimensions, choosing among diagonal/up/left moves based on best score per cell. Backtrace emits `Pair(bookIdx, tgSeq)` only for exact matches (`Eq`) or wildcard matches (`IsWild`), while counting insertions/deletions and match types; diagonal mismatches are traversed but not emitted. It reverses collected pairs and returns `AlignmentResult` with pairs plus aggregate stats.


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

