---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
---
# ValidationReportBuilder::AggregateDiffStats
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`


#### [[ValidationReportBuilder.AggregateDiffStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ValidationReportBuilder.DiffTotals AggregateDiffStats(IEnumerable<HydratedDiffStats> stats)
```

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]
- [[ValidationReportBuilder.BuildWordTallies]]

