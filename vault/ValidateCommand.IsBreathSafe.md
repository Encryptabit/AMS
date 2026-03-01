---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
---
# ValidateCommand::IsBreathSafe
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.IsBreathSafe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsBreathSafe(AudioBuffer audio, double startSec, double endSec)
```

**Calls ->**
- [[FeatureExtraction.Detect]]
- [[AudioProcessor.MeasureRms]]

**Called-by <-**
- [[ValidateCommand.VetPauseAdjustments]]

