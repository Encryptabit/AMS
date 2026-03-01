---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 6
fan_in: 6
fan_out: 4
tags:
  - method
---
# CompressionState::RebuildPreview
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState.RebuildPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void RebuildPreview(PausePolicy basePolicy)
```

**Calls ->**
- [[CompressionControls.ToPolicy]]
- [[PauseCompressionMath.BuildProfiles]]
- [[PauseCompressionMath.ComputeTargetDuration]]
- [[PauseCompressionMath.ShouldPreserve]]

**Called-by <-**
- [[CompressionState..ctor]]
- [[CompressionState.AdjustSelectedControl]]
- [[CompressionState.ApplyPreview]]
- [[CompressionState.HandleCommit]]
- [[CompressionState.NotifyPauseAdjusted]]
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

