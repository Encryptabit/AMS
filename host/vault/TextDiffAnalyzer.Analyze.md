---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# TextDiffAnalyzer::Analyze
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Provides default text-diff analysis with no explicit scoring options.**

This overload is a pass-through convenience method that immediately delegates to `Analyze(referenceText, hypothesisText, null)`. It introduces no independent normalization, tokenization, or scoring behavior; all diff construction and metric computation occurs in the three-argument overload.


#### [[TextDiffAnalyzer.Analyze]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static TextDiffResult Analyze(string referenceText, string hypothesisText)
```

**Calls ->**
- [[TextDiffAnalyzer.Analyze_2]]

**Called-by <-**
- [[TextDiffAnalyzerTests.Analyze_WithoutPhonemeScoring_KeepsSubstitutionPenalty]]

