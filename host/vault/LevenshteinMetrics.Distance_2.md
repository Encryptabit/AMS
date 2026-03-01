---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# LevenshteinMetrics::Distance
**Path**: `Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`

## Summary
**Calculate edit distance between two string-token sequences with configurable string comparison behavior.**

`Distance(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = StringComparison.Ordinal)` computes token-level Levenshtein distance using a 2D DP table sized `(a.Length + 1) x (b.Length + 1)`. It short-circuits when either span is empty, seeds first row/column with incremental insert/delete costs, then fills each cell as the minimum of delete, insert, and substitute operations. Substitution cost is determined by `string.Equals(a[i - 1], b[j - 1], comparisonType)` so caller-selected comparison semantics (e.g., ordinal vs case-insensitive) directly affect edit cost.


#### [[LevenshteinMetrics.Distance_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int Distance(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = Ordinal)
```

**Called-by <-**
- [[LevenshteinMetrics.Similarity]]

