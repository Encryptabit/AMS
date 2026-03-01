---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidationReportBuilder::FormatDiffTotals
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Render aggregate diff counters into a human-readable totals string for the validation report header sections.**

`FormatDiffTotals` converts an aggregated `DiffTotals` value into the report’s totals line string. It first checks `totals.HasAny` and returns `"(diff data unavailable)"` when no meaningful counts exist. Otherwise it computes `matchPct` as `Matches / ReferenceTokens` with a zero-reference fallback of `1.0`, then formats `ref`, `hyp`, `match` (with `P1` percent), insertions, and deletions into a single parenthesized summary.


#### [[ValidationReportBuilder.FormatDiffTotals]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDiffTotals(ValidationReportBuilder.DiffTotals totals)
```

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]

