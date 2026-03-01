---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 14
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::ApplyExactPhonemeEquivalence
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Adjusts diff statistics to treat delete+insert token pairs as matches when their phoneme variants are exactly equivalent.**

`ApplyExactPhonemeEquivalence` post-processes token diff stats by scanning `diffs` with reference/hypothesis cursors and looking for adjacent DELETE+INSERT runs via `TryGetDeleteInsertPair` (substitution-like pairs). For each aligned pair within those runs, it fetches phoneme variants (`GetPhonemes`) and counts exact phoneme matches (`HasExactPhonemeMatch`) as `equivalentPairs`; non-paired diffs only advance cursors by operation type. If no equivalents are found, it returns `stats` unchanged. Otherwise it returns a new `HydratedDiffStats` that increases `Matches` and reduces `Insertions`/`Deletions` by the equivalent count (clamped at zero), effectively removing substitution penalty for exact phoneme-equivalent tokens.


#### [[TextDiffAnalyzer.ApplyExactPhonemeEquivalence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static HydratedDiffStats ApplyExactPhonemeEquivalence(HydratedDiffStats stats, IReadOnlyList<Diff> diffs, IReadOnlyList<string[]> referencePhonemes, IReadOnlyList<string[]> hypothesisPhonemes)
```

**Calls ->**
- [[TextDiffAnalyzer.GetPhonemes]]
- [[TextDiffAnalyzer.HasExactPhonemeMatch]]
- [[TextDiffAnalyzer.TryGetDeleteInsertPair]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

