---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 9
fan_in: 3
fan_out: 0
tags:
  - method
---
# CompressionState::MatchesScope
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState.MatchesScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool MatchesScope(ValidateTimingSession.ScopeEntry scope)
```

**Called-by <-**
- [[InteractiveState.CommitScope]]
- [[CompressionState.HandleCommit]]
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

