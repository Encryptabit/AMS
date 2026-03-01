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
# TextDiffAnalyzer::BuildMetrics
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Builds consolidated sentence alignment metrics from token diff statistics and character-level distance.**

`BuildMetrics` derives sentence-level error metrics from token stats plus character-level CER. It computes WER as `(deletions + insertions) / max(1, referenceTokens)` with explicit empty-reference behavior (`1.0` when hypothesis has tokens, otherwise `0.0`) and caps values at `1.0`; it also computes `spanWer` from deletions only. CER is delegated to `ComputeCer(reference, hypothesis)`. The method returns a `SentenceMetrics` record containing WER, CER, span WER, deletion count, and insertion count.


#### [[TextDiffAnalyzer.BuildMetrics]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SentenceMetrics BuildMetrics(string reference, string hypothesis, HydratedDiffStats stats)
```

**Calls ->**
- [[TextDiffAnalyzer.ComputeCer]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

