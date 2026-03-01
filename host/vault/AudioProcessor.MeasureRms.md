---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioProcessor::MeasureRms
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`

## Summary
**Returns the decibel RMS of an audio interval defined in seconds, with safe handling for invalid/empty ranges.**

`MeasureRms` computes RMS level in dB for a requested time slice by null-checking `buffer`, ordering the input bounds (`lo/hi`), and converting seconds to clamped sample indices via `ClampToBuffer`. If the resulting span is empty (`e <= s`), it returns `double.NegativeInfinity`. Otherwise it calculates linear RMS with `CalculateRms(buffer, s, e - s)` and converts to decibels through `ToDecibels`.


#### [[AudioProcessor.MeasureRms]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double MeasureRms(AudioBuffer buffer, double startSec, double endSec)
```

**Calls ->**
- [[AudioProcessor.CalculateRms]]
- [[AudioProcessor.ClampToBuffer]]
- [[AudioProcessor.ToDecibels]]

**Called-by <-**
- [[ValidateCommand.IsBreathSafe]]
- [[ValidateCommand.VetPauseAdjustments]]

