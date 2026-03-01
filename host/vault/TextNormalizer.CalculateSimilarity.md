---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs"
access_modifier: "public"
complexity: 4
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextNormalizer::CalculateSimilarity
**Path**: `Projects/AMS/host/Ams.Core/Common/TextNormalizer.cs`

## Summary
**Computes a normalized string similarity score by comparing normalized inputs with Levenshtein distance.**

`CalculateSimilarity` first normalizes both inputs via `Normalize`, then fast-paths to `1.0` when normalized strings are identical and to `0.0` when either normalized value is empty. Otherwise it computes Levenshtein edit distance using `LevenshteinMetrics.Distance(normalized1, normalized2)`, derives `maxLength` from the longer normalized string, and returns `1.0 - (double)distance / maxLength`. This yields a bounded similarity score where larger edit distance relative to length lowers the result.


#### [[TextNormalizer.CalculateSimilarity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double CalculateSimilarity(string text1, string text2)
```

**Calls ->**
- [[LevenshteinMetrics.Distance_3]]
- [[TextNormalizer.Normalize]]

**Called-by <-**
- [[ScriptValidator.CalculateMatchCost]]
- [[TextNormalizerTests.CalculateSimilarity_ShouldReturnCorrectSimilarity]]

