---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 6
fan_in: 6
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
---
# CompressionState::RebuildPreview
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Recompute the current preview compression policy from a base pause policy using profile generation, target-duration calculation, and preserve/filter rules.**

`RebuildPreview(PausePolicy basePolicy)` recalculates the compression preview from a supplied base policy by rebuilding profiles (`BuildProfiles`), recomputing the duration target (`ComputeTargetDuration`), applying preservation predicates (`ShouldPreserve`), and projecting the result back to a policy (`ToPolicy`). At complexity 6 and with callers spanning constructor, adjustment, apply, scope-ensure, commit, and notification paths, it serves as the central state-refresh routine that keeps derived preview output consistent with interactive edits.


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

