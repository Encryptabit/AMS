---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
---
# EditablePause::Set
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Set the editable pause duration to a new value so both preview and current-update flows use the same update path.**

EditablePause.Set(double newDuration) is a trivial mutator (complexity 1) that updates the pause duration state by assigning newDuration to the class’s editable duration field/property. It has no branching, async behavior, validation, or error-handling, and acts as the shared mutation point used by ApplyPreview and SetCurrent.


#### [[EditablePause.Set]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Set(double newDuration)
```

**Called-by <-**
- [[CompressionState.ApplyPreview]]
- [[InteractiveState.SetCurrent]]

