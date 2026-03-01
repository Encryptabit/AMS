---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# InteractiveState::NotifyCompressionPauseAdjusted
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.NotifyCompressionPauseAdjusted]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void NotifyCompressionPauseAdjusted(ValidateTimingSession.EditablePause pause)
```

**Calls ->**
- [[CompressionState.NotifyPauseAdjusted]]

**Called-by <-**
- [[InteractiveState.AdjustCurrent]]
- [[InteractiveState.SetCurrent]]

