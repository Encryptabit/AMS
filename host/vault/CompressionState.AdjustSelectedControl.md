---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# CompressionState::AdjustSelectedControl
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Adjust the active compression control during an interactive timing-validation session and refresh the preview when a change occurs.**

`AdjustSelectedControl` applies a multiplier-based adjustment to the currently selected compression control by delegating to `Adjust(deltaMultiplier, basePolicy)`. Its control flow is small (complexity 3) and centers on whether an adjustment was accepted, then triggering `RebuildPreview` to keep the interactive preview consistent with the new state. The method returns a `bool` indicating whether the adjustment was successfully applied.


#### [[CompressionState.AdjustSelectedControl]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool AdjustSelectedControl(double deltaMultiplier, PausePolicy basePolicy)
```

**Calls ->**
- [[CompressionControls.Adjust]]
- [[CompressionState.RebuildPreview]]

**Called-by <-**
- [[InteractiveState.AdjustCompressionControl]]

