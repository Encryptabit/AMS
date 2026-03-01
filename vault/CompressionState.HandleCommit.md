---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# CompressionState::HandleCommit
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState.HandleCommit]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void HandleCommit(ValidateTimingSession.ScopeEntry scope, PausePolicy basePolicy)
```

**Calls ->**
- [[CompressionState.MatchesScope]]
- [[CompressionState.RebuildPreview]]

**Called-by <-**
- [[InteractiveState.CommitScope]]

