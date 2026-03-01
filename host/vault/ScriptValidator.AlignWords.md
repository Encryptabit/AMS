---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 15
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
  - llm/validation
  - llm/utility
---
# ScriptValidator::AlignWords
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.

## Summary
**Computes a cost-aware edit alignment between script words and recognized words for downstream validation/error analysis.**

`AlignWords` performs minimum-cost sequence alignment between `expected` words and `actual` ASR tokens via dynamic programming, using a `double[,] dp` matrix and a parallel `AlignmentOperation[,]` decision matrix sized `(expected.Count + 1, actual.Count + 1)`. It initializes boundary conditions as cumulative insertion/deletion penalties from `_options`, then fills each cell by choosing the cheapest of diagonal substitution/match (`CalculateMatchCost`), deletion, or insertion, marking zero match-cost diagonals as `Match` and non-zero as `Substitute`. It then backtracks from the bottom-right cell to build `AlignmentResult` records (including `ExpectedWord`, `ActualWord`, and per-step `Cost`) and reverses the list to return chronological alignment order.


#### [[ScriptValidator.AlignWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ScriptValidator.AlignmentResult> AlignWords(List<string> expected, List<ScriptValidator.WordAlignment> actual)
```

**Calls ->**
- [[ScriptValidator.CalculateMatchCost]]

**Called-by <-**
- [[ScriptValidator.Validate]]

