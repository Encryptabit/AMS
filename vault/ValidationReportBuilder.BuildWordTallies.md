---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# ValidationReportBuilder::BuildWordTallies
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`


#### [[ValidationReportBuilder.BuildWordTallies]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static WordTallies BuildWordTallies(IReadOnlyList<SentenceView> sentences)
```

**Calls ->**
- [[ValidationReportBuilder.AggregateDiffStats]]
- [[ValidationReportBuilder.ClampToInt]]

**Called-by <-**
- [[ValidationReportBuilder.Build]]

