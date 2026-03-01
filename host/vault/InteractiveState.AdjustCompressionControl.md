---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# InteractiveState::AdjustCompressionControl
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Handle interactive compression-control key input by translating direction/modifier state into a selected-control adjustment and reporting whether it succeeded.**

`AdjustCompressionControl(int direction, ConsoleModifiers modifiers)` is an interactive-state input handler called by `Run` that applies directional compression adjustments based on keyboard modifiers, then delegates the actual mutation to `AdjustSelectedControl`. With complexity 4, its implementation is likely a small branch set for modifier-aware step sizing and handled/unhandled flow, returning `bool` to report whether a compression control update was performed.


#### [[InteractiveState.AdjustCompressionControl]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool AdjustCompressionControl(int direction, ConsoleModifiers modifiers)
```

**Calls ->**
- [[CompressionState.AdjustSelectedControl]]

**Called-by <-**
- [[TimingController.Run]]

