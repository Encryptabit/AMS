---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs"
access_modifier: "public"
complexity: 23
fan_in: 1
fan_out: 7
tags:
  - method
  - danger/high-complexity
---
# AudioProcessor::SnapToEnergy
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.Analysis.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.


#### [[AudioProcessor.SnapToEnergy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static TimingRange SnapToEnergy(AudioBuffer buffer, TimingRange seed, double enterThresholdDb = -45, double exitThresholdDb = -57, double searchWindowSec = 0.8, double windowMs = 25, double stepMs = 5, double preRollMs = 10, double postRollMs = 10, double hangoverMs = 90)
```

**Calls ->**
- [[AudioProcessor.CalculateRms]]
- [[AudioProcessor.ClampToBuffer]]
- [[AudioProcessor.DbToLinear]]
- [[ResolveStart]]
- [[RmsBackward]]
- [[RmsForward]]
- [[WindowRms]]

**Called-by <-**
- [[SpliceBoundaryService.RefineBoundary]]

