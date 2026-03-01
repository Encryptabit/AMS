---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ScriptValidator::CalculateEditDistance
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Computes the minimum number of token edits required to transform one sequence into another for WER/CER validation metrics.**

`CalculateEditDistance` implements a standard Levenshtein dynamic-programming routine over `string[]` tokens. It creates an `int[,]` matrix of size `(s1.Length + 1) x (s2.Length + 1)`, initializes the first row/column with incremental insert/delete costs, then fills each cell with the minimum of delete, insert, or substitute/match (`cost = 0` when `s1[i-1] == s2[j-1]`, otherwise `1`). It returns `dp[s1.Length, s2.Length]`, which `CalculateSegmentWER` and `CalculateCharacterErrorRate` divide by expected length to produce normalized error rates.


#### [[ScriptValidator.CalculateEditDistance]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private int CalculateEditDistance(string[] s1, string[] s2)
```

**Called-by <-**
- [[ScriptValidator.CalculateCharacterErrorRate]]
- [[ScriptValidator.CalculateSegmentWER]]

