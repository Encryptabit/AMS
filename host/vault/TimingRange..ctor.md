---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/TimingRange.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# TimingRange::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/TimingRange.cs`

## Summary
**Create a validated, normalized, microsecond-precision time interval with guaranteed non-negative duration.**

The `TimingRange` constructor validates numeric inputs, rejecting `NaN` and infinities with `ArgumentException`, then normalizes potentially inverted ranges with a microsecond tolerance (`Precision = 1e-6`). If `endSec + Precision < startSec`, it clamps `endSec` to `startSec` instead of throwing, yielding a non-negative interval. It assigns immutable `StartSec`/`EndSec` after rounding to 6 decimal places via `Round(..., MidpointRounding.AwayFromZero)`, with `EndSec` additionally guarded by `Math.Max(startSec, endSec)`.


#### [[TimingRange..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TimingRange(double startSec, double endSec)
```

**Calls ->**
- [[TimingRange.Round]]

