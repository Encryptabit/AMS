---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 24
fan_in: 1
fan_out: 6
tags:
  - method
  - danger/high-complexity
---
# ValidationReportBuilder::BuildTextReport
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

> [!danger] High Complexity (24)
> Cyclomatic complexity: 24. Consider refactoring into smaller methods.


#### [[ValidationReportBuilder.BuildTextReport]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildTextReport(SourceInfo info, IReadOnlyList<SentenceView> sentences, IReadOnlyList<ParagraphView> paragraphs, WordTallies wordTallies, ValidationReportOptions options, HydratedTranscript hydrated)
```

**Calls ->**
- [[ValidationReportBuilder.AggregateDiffStats]]
- [[ValidationReportBuilder.AppendDiffOps]]
- [[ValidationReportBuilder.FormatDiffStats]]
- [[ValidationReportBuilder.FormatDiffTotals]]
- [[ValidationReportBuilder.HasParagraphDiffIssues]]
- [[ValidationReportBuilder.TrimText]]

**Called-by <-**
- [[ValidationReportBuilder.Build]]

