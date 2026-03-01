---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidationReportBuilder::FormatTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`


#### [[ValidationReportBuilder.FormatTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatTokens(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[ValidationReportBuilder.TrimText]]

**Called-by <-**
- [[ValidationReportBuilder.AppendDiffOps]]

