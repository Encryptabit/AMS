---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::NotifyCompressionPauseAdjusted
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Provide a compression-pause-specific notification entry that delegates to the common pause-adjusted notifier.**

NotifyCompressionPauseAdjusted is a private helper on InteractiveState that routes a compression-specific pause change into the shared NotifyPauseAdjusted notification path. Its low complexity and single callee indicate it is a thin specialization layer rather than containing business logic itself. Being called from both AdjustCurrent and SetCurrent centralizes and normalizes pause-adjustment signaling across those edit flows.


#### [[InteractiveState.NotifyCompressionPauseAdjusted]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void NotifyCompressionPauseAdjusted(ValidateTimingSession.EditablePause pause)
```

**Calls ->**
- [[CompressionState.NotifyPauseAdjusted]]

**Called-by <-**
- [[InteractiveState.AdjustCurrent]]
- [[InteractiveState.SetCurrent]]

