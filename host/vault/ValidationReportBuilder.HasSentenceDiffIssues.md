---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ValidationReportBuilder::HasSentenceDiffIssues
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Determine whether a sentence has diff-related issues (or non-ok status when diff stats are unavailable).**

`HasSentenceDiffIssues` classifies whether a sentence should be treated as problematic for report filtering. It first inspects `sentence.Diff?.Stats`; if stats are absent, it falls back to status-based logic and flags any sentence whose `Status` is not `"ok"` (case-insensitive). If stats exist, it flags issues when either `Insertions` or `Deletions` is greater than zero, ignoring match count alone.


#### [[ValidationReportBuilder.HasSentenceDiffIssues]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasSentenceDiffIssues(SentenceView sentence)
```

