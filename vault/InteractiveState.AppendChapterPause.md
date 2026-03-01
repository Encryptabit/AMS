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
# InteractiveState::AppendChapterPause
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.AppendChapterPause]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendChapterPause(List<ValidateTimingSession.ScopeEntry> entries, ValidateTimingSession.EditablePause pause)
```

**Calls ->**
- [[InteractiveState.BuildPauseLabel]]

**Called-by <-**
- [[InteractiveState.BuildEntries]]

