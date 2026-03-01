---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateTimingSession::BuildStaticBufferAdjustments
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build the static-buffer pause adjustment set by reconciling baseline timeline data with dynamic pause adjustments.**

`BuildStaticBufferAdjustments` is a private helper in `ValidateTimingSession` that computes static `PauseAdjust` values from the interactive session state plus previously calculated dynamic adjustments. It pulls a baseline model via `GetBaselineTimeline(state)` and uses `Apply(...)` to project dynamic changes onto that baseline before extracting the static buffer-related adjustments it returns as `IReadOnlyList<PauseAdjust>`. Its complexity (5) suggests a few focused branches for timing/buffer edge handling, and it feeds `BuildAdjustmentsIncludingStatic`.


#### [[ValidateTimingSession.BuildStaticBufferAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<PauseAdjust> BuildStaticBufferAdjustments(ValidateTimingSession.InteractiveState state, IReadOnlyList<PauseAdjust> dynamicAdjustments)
```

**Calls ->**
- [[InteractiveState.GetBaselineTimeline]]
- [[PauseTimelineApplier.Apply]]

**Called-by <-**
- [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]

