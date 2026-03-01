---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
---
# ValidationReportBuilder::ComputeDiffScore
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`


#### [[ValidationReportBuilder.ComputeDiffScore]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeDiffScore(HydratedDiffStats stats, double fallback)
```

**Called-by <-**
- [[ValidationReportBuilder.ComputeParagraphDiffScore]]
- [[ValidationReportBuilder.ComputeSentenceDiffScore]]

