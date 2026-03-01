---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/utility
---
# CompressionControls::SetRatioOutside
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Determine whether a proposed compression ratio is outside the allowed range.**

`SetRatioOutside(double value)` is a private predicate in `CompressionControls` with cyclomatic complexity 2, indicating a single decision branch in implementation. It likely performs a bounds check against the control’s configured ratio limits and returns `true` when `value` is out of range, which `Adjust` uses to decide its next path.


#### [[CompressionControls.SetRatioOutside]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool SetRatioOutside(double value)
```

**Called-by <-**
- [[CompressionControls.Adjust]]

