---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# CompressionState::AdjustSelectedControl
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState.AdjustSelectedControl]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool AdjustSelectedControl(double deltaMultiplier, PausePolicy basePolicy)
```

**Calls ->**
- [[CompressionControls.Adjust]]
- [[CompressionState.RebuildPreview]]

**Called-by <-**
- [[InteractiveState.AdjustCompressionControl]]

