---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 16
fan_in: 3
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# TranscriptAligner::AlignWindows
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.

## Summary
**Computes token-level alignment operations across predefined windows using weighted edit-distance DP with lexical and optional phoneme-aware substitution costs.**

AlignWindows runs per-window dynamic-programming alignment over normalized token slices, where each window is treated as half-open (`n = bHi - bLo`, `m = aHi - aLo`) and empty-side edge cases are allowed. For each window it builds `dp`/`bt` matrices with costs from `SubCost`, `DelCost`, and `InsCost`, selecting the minimum-cost move (diag/up/left) at each cell. Backtrace emits ordered ops as `(bi, aj, AlignOp, reason, score)`: `Match`/`Sub` on diagonal with reason `equal_or_equiv` or `near_or_diff`, `Del` as `missing_book`, and `Ins` as `filler`/`extra` depending on filler membership. It concatenates window results in input order; although `maxRun` and `maxAvg` are parameters and counters are tracked, no threshold enforcement is currently applied in this method.


#### [[TranscriptAligner.AlignWindows]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<(int? bi, int? aj, AlignOp op, string reason, double score)> AlignWindows(IReadOnlyList<string> bookNorm, IReadOnlyList<string> asrNorm, IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> windows, IReadOnlyDictionary<string, string> equiv, ISet<string> fillers, IReadOnlyList<string[]> bookPhonemes = null, IReadOnlyList<string[]> asrPhonemes = null, int maxRun = 8, double maxAvg = 0.6, double phonemeSoftThreshold = 1.01)
```

**Calls ->**
- [[TranscriptAligner.DelCost]]
- [[TranscriptAligner.GetPhonemes]]
- [[TranscriptAligner.InsCost]]
- [[TranscriptAligner.SubCost]]

**Called-by <-**
- [[TranscriptIndexService.BuildWordOperations]]
- [[TxAlignTests.Align_PhonemeMatchTreatsHomophoneAsMatch]]
- [[TxAlignTests.Align_SimpleNearMatch_YieldsSubNotDelIns]]

