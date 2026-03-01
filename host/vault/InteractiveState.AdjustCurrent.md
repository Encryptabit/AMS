---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# InteractiveState::AdjustCurrent
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Adjust the currently selected timing value by a delta and notify compression-pause listeners when applicable, returning whether the operation succeeded.**

`AdjustCurrent` is a synchronous state-mutation method that applies `deltaSeconds` to the active timing target by delegating core update logic to `Adjust(...)`. Its `bool` return indicates whether the adjustment was accepted/applied, with branching (complexity 5) likely handling guard conditions around current selection and allowable deltas. When the affected target is a compression pause, it invokes `NotifyCompressionPauseAdjusted(...)` to publish the adjustment side effect.


#### [[InteractiveState.AdjustCurrent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool AdjustCurrent(double deltaSeconds)
```

**Calls ->**
- [[EditablePause.Adjust]]
- [[InteractiveState.NotifyCompressionPauseAdjusted]]

**Called-by <-**
- [[TimingController.AdjustCurrent]]

