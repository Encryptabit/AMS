---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TimingRange.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# TimingRange::WithStart
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TimingRange.cs`

## Summary
**Create a new time range with an updated start time and the existing end time.**

`WithStart` returns a new `TimingRange` via `new(startSec, EndSec)`, replacing only the start boundary while preserving the current end input. Because it routes through the constructor, it inherits the same input validation (NaN/infinity), tolerance-based ordering normalization, and 6-decimal rounding behavior. The original record instance remains unchanged.


#### [[TimingRange.WithStart]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TimingRange WithStart(double startSec)
```

