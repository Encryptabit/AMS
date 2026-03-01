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
  - llm/validation
  - llm/utility
  - llm/error-handling
---
# InteractiveState::SetCurrent
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Apply a user-provided duration to the interactive session state and emit related pause-adjustment side effects when needed.**

`SetCurrent(double newDuration)` in `Ams.Cli.Commands.ValidateTimingSession.InteractiveState` is a small state-transition wrapper used by `PromptForValue`. Its control flow (complexity 5) suggests guarded mutation: it checks/handles the proposed duration, delegates the core assignment to `Set`, and conditionally triggers `NotifyCompressionPauseAdjusted` when the update affects compression-pause timing. The boolean return value communicates whether the update was accepted/applied so the caller can handle invalid or rejected input paths.


#### [[InteractiveState.SetCurrent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool SetCurrent(double newDuration)
```

**Calls ->**
- [[EditablePause.Set]]
- [[InteractiveState.NotifyCompressionPauseAdjusted]]

**Called-by <-**
- [[TimingController.PromptForValue]]

