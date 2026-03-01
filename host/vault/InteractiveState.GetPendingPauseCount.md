---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::GetPendingPauseCount
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return the number of pending pauses for the given timing-session scope entry.**

In `InteractiveState`, `GetPendingPauseCount(ValidateTimingSession.ScopeEntry scope)` is a minimal helper invoked by `BuildOptionsPanel` to expose pause status in the interactive validation UI. Its logic is effectively a thin delegation to `CollectPauses(scope)` followed by deriving and returning an `int` pending-count result, with no meaningful control flow beyond that (complexity 1).


#### [[InteractiveState.GetPendingPauseCount]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public int GetPendingPauseCount(ValidateTimingSession.ScopeEntry scope)
```

**Calls ->**
- [[InteractiveState.CollectPauses]]

**Called-by <-**
- [[TimingRenderer.BuildOptionsPanel]]

