---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/validation
  - llm/utility
---
# ValidateCommand::IsBreathSafe
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Determines whether a proposed audio interval is safe to treat as pause content by screening for detectable breath signal and RMS intensity.**

`IsBreathSafe` is a private static validator used by `VetPauseAdjustments` to evaluate an `AudioBuffer` interval (`startSec` to `endSec`) for breath-related risk. Its implementation combines `Detect(...)` (breath/event presence) with `MeasureRms(...)` (window energy) and applies branch-heavy gating logic (complexity 8) with early exits. The method returns `true` only when the segment satisfies both detection and loudness safety conditions.


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

