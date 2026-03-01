---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidationReportBuilder::FormatTokens
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Format a token sequence into a compact, human-readable string with empty and length-limited handling.**

`FormatTokens` converts a token list into a display string for diff-op reporting. It returns `"(empty)"` when `tokens.Count == 0`; otherwise it joins tokens with single spaces and passes the result to `TrimText(joined, 80)`. This enforces a concise, bounded line length for operation previews.


#### [[ValidationReportBuilder.FormatTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatTokens(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[ValidationReportBuilder.TrimText]]

**Called-by <-**
- [[ValidationReportBuilder.AppendDiffOps]]

