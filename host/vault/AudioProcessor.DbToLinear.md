---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioProcessor::DbToLinear
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`

## Summary
**Converts dB thresholds into linear amplitude values for signal-energy comparisons.**

`DbToLinear` maps a decibel value to linear amplitude with `Math.Pow(10.0, db / 20.0)`. It applies a floor guard by returning `0.0` when `db <= -120`, avoiding tiny sub-noise values and stabilizing threshold math.


#### [[AudioProcessor.DbToLinear]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double DbToLinear(double db)
```

**Called-by <-**
- [[AudioProcessor.SnapToEnergy]]

