---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::ResolveScoringTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Builds the scoring token sequence from optional caller-supplied tokens, or tokenizes normalized fallback text when none are supplied.**

`ResolveScoringTokens` selects the token source for scoring: when `providedTokens` is null it falls back to `Tokenize(normalizedFallback)`. If tokens are provided, it builds a new list, skips null/whitespace entries, trims each remaining token, and preserves input order. The result is a clean mutable token list for scoring calculations.


#### [[TextDiffAnalyzer.ResolveScoringTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> ResolveScoringTokens(string normalizedFallback, IReadOnlyList<string> providedTokens)
```

**Calls ->**
- [[TextDiffAnalyzer.Tokenize]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

