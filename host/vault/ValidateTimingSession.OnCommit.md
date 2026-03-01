---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/validation
  - llm/data-access
  - llm/error-handling
  - llm/utility
---
# ValidateTimingSession::OnCommit
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Finalize an interactive validation commit by persisting pause adjustments and updating the session’s last commit message/result state.**

`OnCommit` is a private post-commit handler in `ValidateTimingSession` that is invoked by `RunAsync` with the current `InteractiveState` and `CommitResult`. Its implementation is orchestration-focused: it calls `PersistPauseAdjustments` and `UpdateLastCommitMessage` to finalize commit-side effects. The low cyclomatic complexity (3) implies only light branching around these operations.


#### [[ValidateTimingSession.OnCommit]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void OnCommit(ValidateTimingSession.InteractiveState state, ValidateTimingSession.CommitResult result)
```

**Calls ->**
- [[InteractiveState.UpdateLastCommitMessage]]
- [[ValidateTimingSession.PersistPauseAdjustments]]

**Called-by <-**
- [[ValidateTimingSession.RunAsync]]

