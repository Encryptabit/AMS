---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# InteractiveState::SetCurrent
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.SetCurrent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool SetCurrent(double newDuration)
```

**Calls ->**
- [[EditablePause.Set]]
- [[InteractiveState.NotifyCompressionPauseAdjusted]]

**Called-by <-**
- [[TimingController.PromptForValue]]

