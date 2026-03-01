---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::ComputeCer
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Computes normalized character error rate between reference and hypothesis strings from semantic diff operations.**

`ComputeCer` calculates character error rate using `diff_match_patch`: it handles the empty-reference edge case first (`0.0` if hypothesis is also empty, else `1.0`), then computes semantic diffs between `reference` and `hypothesis`. It accumulates equal-character and inserted-character counts from diff operations and derives CER as `((reference.Length - equalChars) + insertChars) / max(1, reference.Length)`, capped at `1.0`. This effectively penalizes substitutions/deletions via missing equals and insertions explicitly.


#### [[TextDiffAnalyzer.ComputeCer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeCer(string reference, string hypothesis)
```

**Called-by <-**
- [[TextDiffAnalyzer.BuildMetrics]]

