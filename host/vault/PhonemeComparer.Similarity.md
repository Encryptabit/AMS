---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PhonemeComparer::Similarity
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Produces a case-insensitive Levenshtein-based similarity score between two phoneme token sequences.**

PhonemeComparer.Similarity computes normalized similarity between two phoneme-token arrays. It returns `0.0` when either array is empty, otherwise delegates to `LevenshteinMetrics.Similarity(a.AsSpan(), b.AsSpan(), StringComparison.OrdinalIgnoreCase)` for case-insensitive sequence comparison. The output is a continuous score used by soft phoneme matching thresholds.


#### [[PhonemeComparer.Similarity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double Similarity(string[] a, string[] b)
```

**Calls ->**
- [[LevenshteinMetrics.Similarity]]

**Called-by <-**
- [[TranscriptAligner.HasSoftPhonemeMatch]]

