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
# TextDiffAnalyzer::BuildStats
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Computes match/insert/delete token statistics from diff operations for downstream scoring.**

`BuildStats` aggregates token-level diff counts by iterating `diffs`, treating each diff’s `text.Length` as token cardinality and skipping zero-length entries. It increments matches for `Operation.EQUAL`, insertions for `Operation.INSERT`, and deletions for `Operation.DELETE`, then returns a `HydratedDiffStats` initialized with the provided reference/hypothesis token totals plus computed counts. The method performs no normalization itself; it summarizes already token-encoded diff output.


#### [[TextDiffAnalyzer.BuildStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static HydratedDiffStats BuildStats(int referenceTokenCount, int hypothesisTokenCount, IReadOnlyList<Diff> diffs)
```

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

