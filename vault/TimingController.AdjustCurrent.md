---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# TimingController::AdjustCurrent
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[TimingController.AdjustCurrent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool AdjustCurrent(int direction, ConsoleModifiers modifiers)
```

**Calls ->**
- [[InteractiveState.AdjustCurrent]]

**Called-by <-**
- [[TimingController.Run]]

