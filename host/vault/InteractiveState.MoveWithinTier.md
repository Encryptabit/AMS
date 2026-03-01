---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::MoveWithinTier
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Move the current interactive selection within its tier by `delta` and synchronize visibility/compression state when the move is valid.**

`MoveWithinTier(int delta)` in `ValidateTimingSession.InteractiveState` appears to perform bounded, branch-heavy intra-tier navigation (complexity 7), applying a relative move and returning a success flag. After adjusting position, it invokes `EnsureTreeVisibility` to keep the active element visible in the session tree and `RefreshCompressionStateIfNeeded` to recompute compression/collapse state only when necessary. Being called by `Run`, it functions as a state-transition primitive for interactive command flow rather than a top-level entry point.


#### [[InteractiveState.MoveWithinTier]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool MoveWithinTier(int delta)
```

**Calls ->**
- [[InteractiveState.EnsureTreeVisibility]]
- [[InteractiveState.RefreshCompressionStateIfNeeded]]

**Called-by <-**
- [[TimingController.Run]]

