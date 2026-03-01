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
# ValidationReportBuilder::TrimText
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Normalize multiline text into a single-line representation and apply optional ellipsis-based length limiting.**

`TrimText` normalizes and optionally truncates arbitrary text for report output. It replaces `\n`/`\r` with spaces and trims outer whitespace, then returns early when `maxLength` is null or the normalized text already fits. If truncation is required, it slices to `maxLength`, trims trailing whitespace, and appends `"..."`.


#### [[ValidationReportBuilder.TrimText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TrimText(string text, int? maxLength = null)
```

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]
- [[ValidationReportBuilder.FormatTokens]]

