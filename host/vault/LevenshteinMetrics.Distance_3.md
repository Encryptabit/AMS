---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# LevenshteinMetrics::Distance
**Path**: `Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`

## Summary
**Compute character-level Levenshtein edit distance for two strings by delegating to the span overload after null validation.**

`Distance(string a, string b)` is a thin null-safe wrapper over the span-based Levenshtein implementation. It validates both inputs with `ArgumentNullException.ThrowIfNull`, converts them to `ReadOnlySpan<char>` via `AsSpan()`, and delegates to `Distance(ReadOnlySpan<char>, ReadOnlySpan<char>)`. The method performs no direct DP work itself and exists to provide a string-friendly API surface.


#### [[LevenshteinMetrics.Distance_3]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int Distance(string a, string b)
```

**Calls ->**
- [[LevenshteinMetrics.Distance]]

**Called-by <-**
- [[LevenshteinMetrics.Similarity_2]]
- [[TextNormalizer.CalculateSimilarity]]

