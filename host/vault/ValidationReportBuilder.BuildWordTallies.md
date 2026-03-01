---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/factory
---
# ValidationReportBuilder::BuildWordTallies
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Compute aggregate word-level match/substitution/insertion/deletion totals from sentence diff statistics.**

`BuildWordTallies` short-circuits to `null` when no sentences are present, then aggregates per-sentence diff stats via `AggregateDiffStats(sentences.Select(s => s.Diff?.Stats))` and returns `null` again when the aggregate has no data (`!totals.HasAny`). It derives substitutions as `min(insertions, deletions)`, then computes insertion-only and deletion-only residuals by subtraction. Finally it returns `WordTallies` with all counters normalized through `ClampToInt`, including `Total` from `ReferenceTokens`.


#### [[ValidationReportBuilder.BuildWordTallies]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WordTallies BuildWordTallies(IReadOnlyList<SentenceView> sentences)
```

**Calls ->**
- [[ValidationReportBuilder.AggregateDiffStats]]
- [[ValidationReportBuilder.ClampToInt]]

**Called-by <-**
- [[ValidationReportBuilder.Build]]

