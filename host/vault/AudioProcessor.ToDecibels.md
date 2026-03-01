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
# AudioProcessor::ToDecibels
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`

## Summary
**Transforms a linear signal magnitude into dB scale with safe handling for zero or negative values.**

`ToDecibels` converts a linear amplitude value to decibels using `20.0 * Math.Log10(linear)`. It guards non-positive inputs by returning `double.NegativeInfinity`, representing silence/undefined log-domain magnitude.


#### [[AudioProcessor.ToDecibels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ToDecibels(double linear)
```

**Called-by <-**
- [[AudioProcessor.MeasureRms]]

