---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 1
tags:
  - method
---
# InteractiveState::BuildDiffContext
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.BuildDiffContext]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string BuildDiffContext(ValidateTimingSession.EditablePause pause)
```

**Calls ->**
- [[InteractiveState.TrimAndEscape]]

**Called-by <-**
- [[InteractiveState.DescribePauseContext]]
- [[InteractiveState.TryCreateDiffRow]]

