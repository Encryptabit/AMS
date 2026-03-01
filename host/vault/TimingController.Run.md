---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 23
fan_in: 1
fan_out: 12
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# TimingController::Run
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (23)
> Cyclomatic complexity: 23. Consider refactoring into smaller methods.

## Summary
**Executes the interactive CLI timing-session validation workflow, applying and committing user-driven timing/compression adjustments while managing controller lifecycle.**

`Run()` is the synchronous orchestration loop for `ValidateTimingSession` in `TimingController`, with high branch complexity consistent with input-driven state transitions. Its implementation appears to cycle through UI rendering (`Render`), command dispatch for navigation/focus (`StepInto`, `StepOut`, `MoveWithinTier`, `ToggleOptionsFocus`, compression selection/scroll), and value edits (`AdjustCurrent`, `AdjustCompressionControl`, `PromptForValue`), then persist scoped changes via `CommitCurrentScope`. The explicit `Dispose` call in the method’s call set indicates deterministic cleanup on exit paths.


#### [[TimingController.Run]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Run()
```

**Calls ->**
- [[InteractiveState.AdjustCompressionControl]]
- [[InteractiveState.MoveCompressionControlSelection]]
- [[InteractiveState.MoveWithinTier]]
- [[InteractiveState.ScrollCompressionPreview]]
- [[InteractiveState.StepInto]]
- [[InteractiveState.StepOut]]
- [[TimingController.AdjustCurrent]]
- [[TimingController.CommitCurrentScope]]
- [[TimingController.PromptForValue]]
- [[TimingController.ToggleOptionsFocus]]
- [[TimingRenderer.Dispose]]
- [[TimingRenderer.Render]]

**Called-by <-**
- [[ValidateTimingSession.RunAsync]]

