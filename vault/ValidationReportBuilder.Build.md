---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 5
tags:
  - method
---
# ValidationReportBuilder::Build
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`


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

