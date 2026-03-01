---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::HasExactPhonemeMatch
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Determines whether reference and hypothesis tokens share at least one exactly matching normalized phoneme variant.**

`HasExactPhonemeMatch` checks two phoneme-variant sets for any exact normalized overlap. It first rejects null/empty variant arrays, then iterates each non-blank reference and hypothesis variant, normalizes each via `NormalizePhonemeVariant`, skips variants that normalize to empty, and compares using `StringComparison.OrdinalIgnoreCase`. It returns `true` on the first matching normalized pair, otherwise `false`.


#### [[TextDiffAnalyzer.HasExactPhonemeMatch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasExactPhonemeMatch(string[] referenceVariants, string[] hypothesisVariants)
```

**Calls ->**
- [[TextDiffAnalyzer.NormalizePhonemeVariant]]

**Called-by <-**
- [[TextDiffAnalyzer.ApplyExactPhonemeEquivalence]]

