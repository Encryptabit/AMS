---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# CompressionState::NotifyPauseAdjusted
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

