---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 20
fan_in: 2
fan_out: 5
tags:
  - method
  - danger/high-complexity
---
# InteractiveState::CommitScope
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.


#### [[InteractiveState.CommitScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.CommitResult CommitScope(ValidateTimingSession.ScopeEntry scope, ValidateTimingSession.CompressionApplySummary summary = null)
```

**Calls ->**
- [[EditablePause.Commit]]
- [[InteractiveState.CollectPauses]]
- [[CompressionState.HandleCommit]]
- [[CompressionState.MatchesScope]]
- [[InteractiveState.MatchesCommittedPause]]

**Called-by <-**
- [[ValidateTimingSession.RunHeadlessAsync]]
- [[TimingController.CommitCurrentScope]]

