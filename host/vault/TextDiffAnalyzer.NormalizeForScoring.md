---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::NormalizeForScoring
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Produces a normalized scoring string with contraction expansion and null/whitespace collapse to empty.**

`NormalizeForScoring` canonicalizes input text for metric computation by returning `string.Empty` when the value is null/whitespace, otherwise delegating to `TextNormalizer.Normalize` with `expandContractions: true` and `removeNumbers: false`. This yields a scoring-focused normalization path that expands contractions while preserving numeric content.


#### [[TextDiffAnalyzer.NormalizeForScoring]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeForScoring(string value)
```

**Calls ->**
- [[TextNormalizer.Normalize]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

