---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# InteractiveState::AdjustCompressionControl
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.AdjustCompressionControl]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool AdjustCompressionControl(int direction, ConsoleModifiers modifiers)
```

**Calls ->**
- [[CompressionState.AdjustSelectedControl]]

**Called-by <-**
- [[TimingController.Run]]

