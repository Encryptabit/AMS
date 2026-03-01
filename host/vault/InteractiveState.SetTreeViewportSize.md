---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::SetTreeViewportSize
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Set the tree viewport size and immediately reconcile visibility and compression state for the interactive tree view.**

`SetTreeViewportSize(int size)` is a low-complexity (`2`) state-update method in `InteractiveState` that applies a new tree viewport size during interactive rendering. Its implementation flow includes `EnsureTreeVisibility()` to keep the current tree focus visible after sizing changes, then `RefreshCompressionStateIfNeeded()` to recalculate compression based on the new viewport constraints. It is invoked by `BuildTree`, so viewport normalization is part of tree construction.


#### [[InteractiveState.SetTreeViewportSize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetTreeViewportSize(int size)
```

**Calls ->**
- [[InteractiveState.EnsureTreeVisibility]]
- [[InteractiveState.RefreshCompressionStateIfNeeded]]

**Called-by <-**
- [[TimingRenderer.BuildTree]]

