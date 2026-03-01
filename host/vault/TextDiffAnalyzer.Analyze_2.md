---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "public"
complexity: 11
fan_in: 3
fan_out: 11
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::Analyze
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Produces normalized token-level diff output and scoring metrics (optionally phoneme-adjusted) between reference and hypothesis text.**

`Analyze(referenceText, hypothesisText, scoringOptions)` runs dual normalization/token pipelines: one for scoring (`NormalizeForScoring` + `ResolveScoringTokens`) and one for reviewer-facing output (`NormalizeForDisplay` + `Tokenize`). It aligns optional phoneme payloads (`AlignPhonemePayload`), builds token diffs for display and scoring (`BuildTokenDiff`), converts display diffs into hydrated operations via `DecodeTokens` + `MapOperation`, and computes stats with `BuildStats`. If `UseExactPhonemeEquivalence` is enabled, it rewrites scoring stats through `ApplyExactPhonemeEquivalence` before metrics are derived via `BuildMetrics`. It returns `TextDiffResult` containing sentence metrics, hydrated diff payload, and deletion-based coverage computed from scoring stats.


#### [[TextDiffAnalyzer.Analyze_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static TextDiffResult Analyze(string referenceText, string hypothesisText, TextDiffScoringOptions scoringOptions)
```

**Calls ->**
- [[TextDiffAnalyzer.AlignPhonemePayload]]
- [[TextDiffAnalyzer.ApplyExactPhonemeEquivalence]]
- [[TextDiffAnalyzer.BuildMetrics]]
- [[TextDiffAnalyzer.BuildStats]]
- [[TextDiffAnalyzer.BuildTokenDiff]]
- [[TextDiffAnalyzer.DecodeTokens]]
- [[TextDiffAnalyzer.MapOperation]]
- [[TextDiffAnalyzer.NormalizeForDisplay]]
- [[TextDiffAnalyzer.NormalizeForScoring]]
- [[TextDiffAnalyzer.ResolveScoringTokens]]
- [[TextDiffAnalyzer.Tokenize]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze]]
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]
- [[TextDiffAnalyzerTests.Analyze_WithExactPhonemeEquivalence_RemovesSubstitutionPenalty]]

