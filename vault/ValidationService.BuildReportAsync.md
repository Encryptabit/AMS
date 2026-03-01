---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidationService::BuildReportAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationService.cs`


#### [[ValidationService.BuildReportAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<ReportResult> BuildReportAsync(ChapterContext chapter, ValidationReportOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[ValidationReportBuilder.Build]]

**Called-by <-**
- [[ValidateCommand.CreateReportCommand]]

