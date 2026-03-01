---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 8
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# LevenshteinMetrics::Distance
**Path**: `Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`

## Summary
**Calculate the minimum number of single-character edits required to transform one char span into another.**

`Distance(ReadOnlySpan<char> a, ReadOnlySpan<char> b)` computes classic Levenshtein edit distance using dynamic programming over a `(a.Length + 1) x (b.Length + 1)` integer matrix. It short-circuits empty-span cases by returning the other length, initializes first row/column as insertion/deletion baselines, then fills the matrix with `min(delete, insert, substitute)` where substitution cost is `0` for equal chars and `1` otherwise. The final distance is read from `dp[a.Length, b.Length]`.


#### [[LevenshteinMetrics.Distance]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int Distance(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
```

**Called-by <-**
- [[LevenshteinMetrics.Distance_3]]
- [[TranscriptAligner.ComputeCer]]

