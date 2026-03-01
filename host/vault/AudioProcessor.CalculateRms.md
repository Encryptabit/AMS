---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioProcessor::CalculateRms
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`

## Summary
**Calculates the linear root-mean-square amplitude for a bounded region of an audio buffer.**

`CalculateRms` computes linear RMS over a sample window across all channels by summing squared amplitudes from `startSample` to `startSample + length` (clamped per-channel by span length) and counting contributing samples. It returns `0.0` immediately for non-positive `length`, and also when no samples were counted. Otherwise it returns `Math.Sqrt(sum / count)`, yielding channel-inclusive RMS energy for the requested window.


#### [[AudioProcessor.CalculateRms]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double CalculateRms(AudioBuffer buffer, int startSample, int length)
```

**Called-by <-**
- [[AudioProcessor.MeasureRms]]
- [[AudioProcessor.SnapToEnergy]]

