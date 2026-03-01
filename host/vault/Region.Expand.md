---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# Region::Expand
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Create an expanded time region around the current span, with the start floored at 0 seconds.**

`Expand` returns a new `Region` with symmetric padding by subtracting `sec` from `StartSec` and adding `sec` to `EndSec`. The start boundary is clamped to zero via `Math.Max(0d, StartSec - sec)` to avoid negative time, while the end is not clamped (`EndSec + sec`). It is a non-mutating value-style helper on the `readonly record struct`.


#### [[Region.Expand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Region Expand(double sec)
```

