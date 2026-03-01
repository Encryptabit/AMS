---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioProcessor::ClampToBuffer
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`

## Summary
**Maps a time in seconds to a safe in-range sample position for a given audio buffer.**

`ClampToBuffer` converts a time offset in seconds to a sample index by rounding `sec * buffer.SampleRate` to the nearest integer. It then bounds the result to valid buffer limits with `Math.Clamp(..., 0, buffer.Length)`, ensuring callers never index outside the audio span.


#### [[AudioProcessor.ClampToBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int ClampToBuffer(AudioBuffer buffer, double sec)
```

**Called-by <-**
- [[AudioProcessor.MeasureRms]]
- [[AudioProcessor.SnapToEnergy]]

