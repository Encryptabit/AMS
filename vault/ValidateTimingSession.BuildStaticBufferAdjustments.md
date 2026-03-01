---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# ValidateTimingSession::BuildStaticBufferAdjustments
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

