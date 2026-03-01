---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 5
tags:
  - method
---
# ValidateTimingSession::PersistPauseAdjustments
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.PersistPauseAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<PauseAdjust> PersistPauseAdjustments(ValidateTimingSession.InteractiveState state)
```

**Calls ->**
- [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]
- [[ValidateTimingSession.GetRelativePathSafe]]
- [[InteractiveState.UpdateLastCommitMessage]]
- [[PauseAdjustmentsDocument.Create]]
- [[PauseAdjustmentsDocument.Save]]

**Called-by <-**
- [[ValidateTimingSession.OnCommit]]
- [[ValidateTimingSession.RunHeadlessAsync]]

