---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# ValidationReportBuilder::ClampToInt
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Safely convert a 64-bit count to 32-bit by capping oversized values at `int.MaxValue`.**

`ClampToInt` performs a saturating cast from `long` to `int`. It returns `int.MaxValue` when the input exceeds `int.MaxValue`; otherwise it casts directly to `int`. In current usage (word-tally totals), this prevents overflow from large aggregate counters.


#### [[ValidationReportBuilder.ClampToInt]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int ClampToInt(long value)
```

**Called-by <-**
- [[ValidationReportBuilder.BuildWordTallies]]

