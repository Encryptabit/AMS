---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/validation
  - llm/utility
---
# CompressionState::NotifyPauseAdjusted
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Handle a pause adjustment event in compression validation state and trigger preview regeneration based on the updated pause context.**

`NotifyPauseAdjusted` is a low-complexity state-notification handler (`complexity: 2`) in `InteractiveState.CompressionState` that reacts to edits on an `EditablePause` relative to a `PausePolicy`. Its implementation is lightweight and centered on recomputation, with `RebuildPreview` as its only recorded downstream call. In practice, it serves as an internal bridge from pause-change notifications (via `NotifyCompressionPauseAdjusted`) to preview refresh logic.


#### [[CompressionState.NotifyPauseAdjusted]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void NotifyPauseAdjusted(ValidateTimingSession.EditablePause pause, PausePolicy basePolicy)
```

**Calls ->**
- [[CompressionState.RebuildPreview]]

**Called-by <-**
- [[InteractiveState.NotifyCompressionPauseAdjusted]]

