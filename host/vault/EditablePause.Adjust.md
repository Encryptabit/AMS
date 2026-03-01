---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# EditablePause::Adjust
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Apply a delta in seconds to update the editable pause state.**

`EditablePause.Adjust(double deltaSeconds)` is a synchronous, side-effecting mutator with cyclomatic complexity 1 that performs an O(1) adjustment of pause-related state using the supplied delta and returns no value. Its only listed caller (`AdjustCurrent`) suggests this method is the low-level state-update primitive while higher-level coordination happens in the caller.


#### [[EditablePause.Adjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Adjust(double deltaSeconds)
```

**Called-by <-**
- [[InteractiveState.AdjustCurrent]]

