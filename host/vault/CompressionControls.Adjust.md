---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 9
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# CompressionControls::Adjust
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**It maps a selected control index to the corresponding compression-setting update using `deltaMultiplier` and returns whether the adjustment was valid.**

`Adjust` appears to implement index-based dispatch from the selected compression control to one of four mutators (`SetKneeWidth`, `SetPreserveTopQuantile`, `SetRatioInside`, `SetRatioOutside`), passing `deltaMultiplier` as the adjustment amount. The `bool` return indicates a validation/error path for unsupported indices, while successful cases delegate the actual state update semantics to the target setter. Since it is called by `AdjustSelectedControl`, this method functions as the control-routing layer for interactive compression tuning.


#### [[CompressionControls.Adjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool Adjust(int index, double deltaMultiplier)
```

**Calls ->**
- [[CompressionControls.SetKneeWidth]]
- [[CompressionControls.SetPreserveTopQuantile]]
- [[CompressionControls.SetRatioInside]]
- [[CompressionControls.SetRatioOutside]]

**Called-by <-**
- [[CompressionState.AdjustSelectedControl]]

