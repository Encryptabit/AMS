---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::RefreshCompressionStateIfNeeded
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Refreshes compression state for the current interactive scope, optionally resetting selection-dependent state when requested.**

This private helper is called after navigation/viewport transitions (`MoveWithinTier`, `SetTreeViewportSize`, `StepInto`, `StepOut`) to keep interactive compression state aligned with the active scope. Its control flow is intentionally small (complexity 2): it handles the `resetSelection` flag path and delegates the actual scope reconciliation to `EnsureCompressionStateForCurrentScope`.


#### [[InteractiveState.RefreshCompressionStateIfNeeded]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void RefreshCompressionStateIfNeeded(bool resetSelection)
```

**Calls ->**
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

**Called-by <-**
- [[InteractiveState.MoveWithinTier]]
- [[InteractiveState.SetTreeViewportSize]]
- [[InteractiveState.StepInto]]
- [[InteractiveState.StepOut]]

