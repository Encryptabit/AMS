---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 5
tags:
  - method
---
# InteractiveState::EnsureCompressionStateForCurrentScope
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.EnsureCompressionStateForCurrentScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureCompressionStateForCurrentScope(bool resetSelection)
```

**Calls ->**
- [[InteractiveState.CollectCompressionPauses]]
- [[CompressionControls.FromPolicy]]
- [[CompressionState.MatchesScope]]
- [[CompressionState.RebuildPreview]]
- [[CompressionState.ResetSelection]]

**Called-by <-**
- [[InteractiveState.RefreshCompressionStateIfNeeded]]
- [[InteractiveState.ToggleOptionsFocus]]

