---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
---
# InteractiveState::StepOut
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.StepOut]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool StepOut()
```

**Calls ->**
- [[InteractiveState.EnsureTreeVisibility]]
- [[InteractiveState.RefreshCompressionStateIfNeeded]]

**Called-by <-**
- [[TimingController.Run]]

