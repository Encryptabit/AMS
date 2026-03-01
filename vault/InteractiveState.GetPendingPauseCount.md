---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# InteractiveState::GetPendingPauseCount
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.GetPendingPauseCount]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public int GetPendingPauseCount(ValidateTimingSession.ScopeEntry scope)
```

**Calls ->**
- [[InteractiveState.CollectPauses]]

**Called-by <-**
- [[TimingRenderer.BuildOptionsPanel]]

