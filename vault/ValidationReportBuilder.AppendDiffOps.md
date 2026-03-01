---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidationReportBuilder::AppendDiffOps
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`


#### [[ValidationReportBuilder.AppendDiffOps]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendDiffOps(StringBuilder builder, HydratedDiff diff, string indent, int maxOps = 5)
```

**Calls ->**
- [[ValidationReportBuilder.FormatTokens]]

**Called-by <-**
- [[ValidationReportBuilder.BuildTextReport]]

