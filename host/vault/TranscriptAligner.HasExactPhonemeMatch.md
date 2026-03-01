---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::HasExactPhonemeMatch
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Determines whether any non-empty phoneme variant from the book side exactly matches any variant from the ASR side.**

HasExactPhonemeMatch checks cross-product equality between book and ASR phoneme variant arrays. It first validates both inputs are non-null/non-empty, then skips blank variants and compares remaining pairs via `PhonemeComparer.Equals`, which normalizes/token-compares phoneme strings case-insensitively. The method short-circuits on first exact match and returns `false` if none are found.


#### [[TranscriptAligner.HasExactPhonemeMatch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasExactPhonemeMatch(string[] bookPhonemes, string[] asrPhonemes)
```

**Calls ->**
- [[PhonemeComparer.Equals]]

**Called-by <-**
- [[TranscriptAligner.SubCost]]

