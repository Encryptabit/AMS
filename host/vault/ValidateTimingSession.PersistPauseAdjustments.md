---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 5
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/factory
---
# ValidateTimingSession::PersistPauseAdjustments
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build, persist, and record pause-adjustment artifacts from the current interactive timing state.**

`PersistPauseAdjustments` is a synchronous orchestration helper that derives pause adjustments from `InteractiveState` via `BuildAdjustmentsIncludingStatic`, then materializes canonical `PauseAdjust` instances through `Create`. It resolves a safe relative persistence target with `GetRelativePathSafe`, writes the adjustments with `Save`, and updates commit metadata using `UpdateLastCommitMessage`. The method returns the persisted adjustments as an `IReadOnlyList<PauseAdjust>` for its callers (`OnCommit`, `RunHeadlessAsync`).


#### [[ValidateTimingSession.PersistPauseAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<PauseAdjust> PersistPauseAdjustments(ValidateTimingSession.InteractiveState state)
```

**Calls ->**
- [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]
- [[ValidateTimingSession.GetRelativePathSafe]]
- [[InteractiveState.UpdateLastCommitMessage]]
- [[PauseAdjustmentsDocument.Create]]
- [[PauseAdjustmentsDocument.Save]]

**Called-by <-**
- [[ValidateTimingSession.OnCommit]]
- [[ValidateTimingSession.RunHeadlessAsync]]

