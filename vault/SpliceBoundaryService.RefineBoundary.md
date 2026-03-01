---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 3
tags:
  - method
---
# SpliceBoundaryService::RefineBoundary
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/SpliceBoundaryService.cs`


#### [[SpliceBoundaryService.RefineBoundary]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (double position, BoundaryMethod method) RefineBoundary(AudioBuffer buffer, double bufferDurationSec, double roughSec, double searchLeftSec, double searchRightSec, bool isStartBoundary, SpliceBoundaryOptions opts)
```

**Calls ->**
- [[AudioProcessor.DetectSilence]]
- [[AudioProcessor.SnapToEnergy]]
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[SpliceBoundaryService.RefineBoundaries]]

