---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "public"
complexity: 11
fan_in: 3
fan_out: 11
tags:
  - method
---
# TextDiffAnalyzer::Analyze
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`


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

