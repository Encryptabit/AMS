---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 3
tags:
  - method
---
# AudioProcessor::MeasureRms
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`


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

