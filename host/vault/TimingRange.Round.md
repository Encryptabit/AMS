---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TimingRange.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TimingRange::Round
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TimingRange.cs`

## Summary
**Round a timing value to six decimal places with midpoint rounding away from zero.**

`Round` is a private precision helper that normalizes floating-point timing values using `Math.Round(value, 6, MidpointRounding.AwayFromZero)`. It enforces fixed microsecond-scale resolution (6 decimal places) and deterministic midpoint behavior away from zero. The method is used by the constructor to canonicalize persisted `StartSec`/`EndSec` values.


#### [[TimingRange.Round]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double Round(double value)
```

**Called-by <-**
- [[TimingRange..ctor]]

