---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# LevenshteinMetrics::Similarity
**Path**: `Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`

## Summary
**Convert token-level edit distance into a 0–1 similarity score using the longer sequence length as normalization.**

`Similarity(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = StringComparison.Ordinal)` returns a normalized similarity score derived from Levenshtein distance on token spans. It handles edge cases explicitly: both empty returns `1.0`, one empty returns `0.0`, and a defensive `maxLen == 0` check also returns `1.0`. For non-empty inputs it computes `distance = Distance(a, b, comparisonType)` and maps to similarity with `1.0 - (double)distance / maxLen`, where `maxLen` is the longer span length.


#### [[LevenshteinMetrics.Similarity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double Similarity(ReadOnlySpan<string> a, ReadOnlySpan<string> b, StringComparison comparisonType = Ordinal)
```

**Calls ->**
- [[LevenshteinMetrics.Distance_2]]

**Called-by <-**
- [[PhonemeComparer.Similarity]]

