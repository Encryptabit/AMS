---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::MoveCompressionControlSelection
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Adjust the compression-control selection by `delta` and indicate if the selection update succeeded.**

In `InteractiveState`, `MoveCompressionControlSelection(int delta)` is a small state-transition helper invoked from `Run` to change the active compression-control selection. The implementation delegates movement logic to `MoveControlSelection` and returns its `bool`, propagating whether the selection changed versus a no-op/bounds case. With complexity 3, the method stays lightweight and centers on delegated control-selection movement.


#### [[InteractiveState.MoveCompressionControlSelection]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool MoveCompressionControlSelection(int delta)
```

**Calls ->**
- [[CompressionState.MoveControlSelection]]

**Called-by <-**
- [[TimingController.Run]]

