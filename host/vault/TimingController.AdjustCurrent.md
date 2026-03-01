---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# TimingController::AdjustCurrent
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Moves the current timing selection according to direction/modifier input and indicates whether a valid adjustment was made.**

`AdjustCurrent(int direction, ConsoleModifiers modifiers)` is a private control-flow helper that updates the controller’s current timing position from directional input, with modifier keys influencing how movement is applied. The method’s self-call (`AdjustCurrent` -> `AdjustCurrent`) plus low cyclomatic complexity (2) suggests a small branch with bounded recursive retry/skip behavior, and its `bool` return communicates success/failure back to `Run`.


#### [[TimingController.AdjustCurrent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool AdjustCurrent(int direction, ConsoleModifiers modifiers)
```

**Calls ->**
- [[InteractiveState.AdjustCurrent]]

**Called-by <-**
- [[TimingController.Run]]

