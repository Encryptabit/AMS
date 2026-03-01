---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/error-handling
---
# ValidationService::BuildReportAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationService.cs`

## Summary
**Generate a chapter validation report through `ValidationReportBuilder` while enforcing null-argument and cancellation checks.**

`BuildReportAsync` is a thin service-layer wrapper that performs guard checks (`ArgumentNullException.ThrowIfNull` for `chapter`/`options`) and cooperative cancellation (`cancellationToken.ThrowIfCancellationRequested()`) before building the report. It reads `Transcript` and `HydratedTranscript` from `chapter.Documents`, delegates report construction to `ValidationReportBuilder.Build(...)`, and returns the result via `Task.FromResult`. Despite the async-shaped signature, execution is fully synchronous with no awaited operations.


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

