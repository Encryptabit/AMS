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
# ValidationReportBuilder::FormatDiffStats
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Convert a single diff-stats object into a human-readable metrics string for sentence/paragraph report entries.**

`FormatDiffStats` renders one `HydratedDiffStats` payload into the compact per-item diff summary used in report rows. The implementation guards for missing stats (`null`) by returning `"diff unavailable"`; otherwise it computes match percentage as `Matches / ReferenceTokens` with a zero-reference fallback of `1.0` to avoid division by zero. It then emits a fixed-format string containing reference/hypothesis token counts, matches with `P1` percentage, and insertion/deletion deltas (`+`/`-`).


#### [[ValidationReportBuilder.FormatDiffStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDiffStats(HydratedDiffStats stats)
```

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]

