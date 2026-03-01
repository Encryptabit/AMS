---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::NormalizePhonemeVariant
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Produces a stable token-spaced phoneme string for reliable variant comparison.**

`NormalizePhonemeVariant` canonicalizes a phoneme variant by collapsing null/whitespace input to `string.Empty`; otherwise it tokenizes with `TextNormalizer.TokenizeWords(value)` and rejoins tokens with single spaces. This removes inconsistent spacing/formatting differences while preserving token content for exact-equivalence checks.


#### [[TextDiffAnalyzer.NormalizePhonemeVariant]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizePhonemeVariant(string value)
```

**Called-by <-**
- [[TextDiffAnalyzer.HasExactPhonemeMatch]]

