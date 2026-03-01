---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# ValidateTimingSession::OnCommit
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.OnCommit]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void OnCommit(ValidateTimingSession.InteractiveState state, ValidateTimingSession.CommitResult result)
```

**Calls ->**
- [[InteractiveState.UpdateLastCommitMessage]]
- [[ValidateTimingSession.PersistPauseAdjustments]]

**Called-by <-**
- [[ValidateTimingSession.RunAsync]]

