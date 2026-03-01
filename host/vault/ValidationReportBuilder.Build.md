---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# ValidationReportBuilder::Build
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Build a complete validation report object from transcript/hydrated inputs, including derived views, optional tallies, and rendered text output.**

`Build` is the orchestration entry for report materialization: it validates inputs by throwing `InvalidOperationException` when both `transcript` and `hydrated` are null, then derives shared metadata via `ExtractSourceInfo`. It constructs sentence and paragraph projections with `BuildSentenceViews`/`BuildParagraphViews`, conditionally computes word tallies when `options.IncludeWordTallies` is true, and generates the formatted text payload through `BuildTextReport(info, sentences, paragraphs, tallies, options, hydrated)`. It returns a fully populated `ReportResult` containing the text report plus the computed view/tally collections.


#### [[ValidationReportBuilder.Build]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ReportResult Build(TranscriptIndex transcript, HydratedTranscript hydrated, ValidationReportOptions options)
```

**Calls ->**
- [[ValidationReportBuilder.BuildParagraphViews]]
- [[ValidationReportBuilder.BuildSentenceViews]]
- [[ValidationReportBuilder.BuildTextReport]]
- [[ValidationReportBuilder.BuildWordTallies]]
- [[ValidationReportBuilder.ExtractSourceInfo]]

**Called-by <-**
- [[ValidationService.BuildReportAsync]]

