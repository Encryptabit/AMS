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
# TimingRange::WithEnd
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TimingRange.cs`

## Summary
**Create a new time range with the same start time and a replaced end time.**

`WithEnd` is an immutable updater that returns a new `TimingRange` initialized as `new(StartSec, endSec)`. It preserves the existing start boundary and re-runs all constructor normalization/validation logic (NaN/infinity checks, tolerance-based clamping, rounding) for the updated end value. The current instance is not mutated.


#### [[TimingRange.WithEnd]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TimingRange WithEnd(double endSec)
```

