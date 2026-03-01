---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# CompressionState::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public CompressionState(ValidateTimingSession.ScopeEntry scope, ValidateTimingSession.InteractiveState.CompressionControls controls, List<ValidateTimingSession.EditablePause> pauses, PausePolicy basePolicy)
```

**Calls ->**
- [[CompressionState.RebuildPreview]]

