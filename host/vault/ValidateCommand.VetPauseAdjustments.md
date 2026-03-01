---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 16
fan_in: 0
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/validation
  - llm/utility
---
# ValidateCommand::VetPauseAdjustments
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.

## Summary
**Validate planned pause edits against transcript and audio safety heuristics before downstream use.**

`VetPauseAdjustments` is a private static validation pass that walks `plannedAdjustments` and gates per-item checks through `ShouldVet`. For items that need vetting, it evaluates transcript/audio context with `IsBreathSafe` and `MeasureRms`, using `Debug` calls to trace keep/drop decisions. It returns an `IReadOnlyList<PauseAdjust>` of adjustments that pass those safety checks.


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

