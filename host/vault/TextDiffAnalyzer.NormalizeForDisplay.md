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
# TextDiffAnalyzer::NormalizeForDisplay
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Normalizes text for presentation in diffs without expanding contractions.**

`NormalizeForDisplay` prepares reviewer-facing text by collapsing null/whitespace input to `string.Empty` and otherwise calling `TextNormalizer.Normalize` with `expandContractions: false` and `removeNumbers: false`. Compared with scoring normalization, it preserves contraction surface form for display-oriented diffs while still applying the shared text normalization pipeline.


#### [[TextDiffAnalyzer.NormalizeForDisplay]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeForDisplay(string value)
```

**Calls ->**
- [[TextNormalizer.Normalize]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

