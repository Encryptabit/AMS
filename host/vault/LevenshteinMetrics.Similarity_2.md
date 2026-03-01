---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs"
access_modifier: "public"
complexity: 5
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# LevenshteinMetrics::Similarity
**Path**: `Projects/AMS/host/Ams.Core/Common/LevenshteinMetrics.cs`

## Summary
**Return a 0–1 string similarity score by normalizing Levenshtein edit distance against the longer input length.**

`Similarity(string a, string b)` computes a normalized character-level similarity from Levenshtein distance. It null-checks both inputs with `ArgumentNullException.ThrowIfNull`, handles empties explicitly (`1.0` if both empty, `0.0` if only one empty), then calls `Distance(a, b)` and normalizes by `maxLen = Math.Max(a.Length, b.Length)`. The returned score is `1.0 - (double)distance / maxLen`, yielding a value in `[0,1]` where higher means more similar.


#### [[LevenshteinMetrics.Similarity_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double Similarity(string a, string b)
```

**Calls ->**
- [[LevenshteinMetrics.Distance_3]]

**Called-by <-**
- [[PickupMatchingService.MatchSinglePickupAsync]]
- [[PickupMatchingService.PairSegmentsToTargets]]
- [[PolishVerificationService.RevalidateSegmentAsync]]

