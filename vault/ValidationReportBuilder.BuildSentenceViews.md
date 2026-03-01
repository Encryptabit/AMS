---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 34
fan_in: 1
fan_out: 0
tags:
  - method
  - danger/high-complexity
---
# ValidationReportBuilder::BuildSentenceViews
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

> [!danger] High Complexity (34)
> Cyclomatic complexity: 34. Consider refactoring into smaller methods.


#### [[ValidationReportBuilder.BuildSentenceViews]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<SentenceView> BuildSentenceViews(TranscriptIndex tx, HydratedTranscript hydrated)
```

**Called-by <-**
- [[ValidationReportBuilder.Build]]

