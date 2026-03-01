---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 1
tags:
  - method
---
# InteractiveState::RefreshCompressionStateIfNeeded
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.RefreshCompressionStateIfNeeded]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void RefreshCompressionStateIfNeeded(bool resetSelection)
```

**Calls ->**
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

**Called-by <-**
- [[InteractiveState.MoveWithinTier]]
- [[InteractiveState.SetTreeViewportSize]]
- [[InteractiveState.StepInto]]
- [[InteractiveState.StepOut]]

