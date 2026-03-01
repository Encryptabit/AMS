---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# ValidateTimingSession::BuildAdjustmentsIncludingStatic
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build a persist-ready pause-adjustment list that includes static buffer adjustments and state-derived committed adjustments with structural filtering rules applied.**

`BuildAdjustmentsIncludingStatic` is a low-complexity orchestration method (complexity 2) that assembles the final `IReadOnlyList<PauseAdjust>` for persistence from `ValidateTimingSession.InteractiveState`. Its implementation delegates construction to `BuildStaticBufferAdjustments` and `GetCommittedAdjustments`, then conditionally applies `FilterParagraphZeroAdjustments` based on `IsStructuralClass`. The method keeps decision points minimal and centralizes composition logic before `PersistPauseAdjustments` consumes the result.


#### [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<PauseAdjust> BuildAdjustmentsIncludingStatic(ValidateTimingSession.InteractiveState state)
```

**Calls ->**
- [[ValidateTimingSession.BuildStaticBufferAdjustments]]
- [[InteractiveState.FilterParagraphZeroAdjustments]]
- [[InteractiveState.GetCommittedAdjustments]]
- [[ValidateTimingSession.IsStructuralClass]]

**Called-by <-**
- [[ValidateTimingSession.PersistPauseAdjustments]]

