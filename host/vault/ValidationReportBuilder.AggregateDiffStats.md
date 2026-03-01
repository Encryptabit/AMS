---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidationReportBuilder::AggregateDiffStats
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Aggregate per-segment diff metrics into one `DiffTotals` value for report formatting and word-tally derivation.**

`AggregateDiffStats` performs a single-pass reduction over the provided diff-stat sequence, accumulating `ReferenceTokens`, `HypothesisTokens`, `Matches`, `Insertions`, and `Deletions` into `long` counters. It ignores null entries (`if (stat is null) continue`) and returns a new `DiffTotals` record with the aggregated values. The method is O(n), allocation-light (just the return record), and provides shared totals used by both `BuildTextReport` and `BuildWordTallies`.


#### [[ValidationReportBuilder.AggregateDiffStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ValidationReportBuilder.DiffTotals AggregateDiffStats(IEnumerable<HydratedDiffStats> stats)
```

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]
- [[ValidationReportBuilder.BuildWordTallies]]

