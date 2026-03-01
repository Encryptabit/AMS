---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
---
# AudioProcessor::ClampToBuffer
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`


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

