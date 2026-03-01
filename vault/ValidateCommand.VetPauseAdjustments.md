---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 16
fan_in: 0
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# ValidateCommand::VetPauseAdjustments
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[ValidateCommand.VetPauseAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<PauseAdjust> VetPauseAdjustments(IReadOnlyList<PauseAdjust> plannedAdjustments, TranscriptIndex transcript, AudioBuffer audio)
```

**Calls ->**
- [[ValidateCommand.IsBreathSafe]]
- [[ValidateCommand.ShouldVet]]
- [[Log.Debug]]
- [[AudioProcessor.MeasureRms]]

