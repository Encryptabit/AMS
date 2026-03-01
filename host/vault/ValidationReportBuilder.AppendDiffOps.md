---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ValidationReportBuilder::AppendDiffOps
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Render a bounded, report-friendly preview of non-equal diff operations with graceful handling of missing or uninteresting diff data.**

`AppendDiffOps` appends a compact, human-readable diff-operations block to the report `StringBuilder`. It first guards `diff?.Ops` and writes `Diff ops: (none)` when missing/empty, then filters out `"equal"` operations (case-insensitive) and limits output to `maxOps`. For each remaining op, it writes an indented line with uppercased/padded operation name and token text from `FormatTokens`; if no non-equal ops exist, it emits `Diff ops: (only equal segments)`. When truncated, it appends a final line indicating how many additional ops were omitted.


#### [[ValidationReportBuilder.AppendDiffOps]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendDiffOps(StringBuilder builder, HydratedDiff diff, string indent, int maxOps = 5)
```

**Calls ->**
- [[ValidationReportBuilder.FormatTokens]]

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]

