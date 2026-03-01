---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ValidationReportBuilder::HasParagraphDiffIssues
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Identify paragraph entries that contain diff mismatches, or non-ok status when diff stats are unavailable.**

`HasParagraphDiffIssues` evaluates whether a paragraph should be considered problematic in diff-based reporting. It reads `paragraph.Diff?.Stats`; when stats are missing, it falls back to status semantics and flags any paragraph whose `Status` is not `"ok"` (case-insensitive). When stats are present, it returns true only if `Insertions` or `Deletions` is non-zero, treating pure matches as non-issues.


#### [[ValidationReportBuilder.HasParagraphDiffIssues]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasParagraphDiffIssues(ParagraphView paragraph)
```

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]

