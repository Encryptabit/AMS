---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::MatchesCommittedPause
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Checks whether a committed pause adjustment targets the same pause span as a given `PauseSpan`, using ID equality and epsilon-based time matching.**

`MatchesCommittedPause` is a private static predicate in `ValidateTimingSession.InteractiveState` that determines whether a `PauseAdjust` refers to the same pause span by matching `LeftSentenceId` and `RightSentenceId` exactly, then comparing `StartSec` and `EndSec` with `Math.Abs(...) <= DurationEpsilon`. In `CommitScope`, it is used in `_committedAdjustments.RemoveAll(...)` before adding a new adjustment, so commits replace prior adjustments for the same span instead of accumulating duplicates. The implementation is a constant-time, side-effect-free comparison helper.


#### [[InteractiveState.MatchesCommittedPause]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool MatchesCommittedPause(PauseAdjust adjust, PauseSpan span)
```

**Called-by <-**
- [[InteractiveState.CommitScope]]

