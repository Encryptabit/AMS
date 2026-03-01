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
  - llm/error-handling
---
# CompressionControls::SetRatioInside
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Validate a proposed compression ratio and apply it to interactive state only when it falls within the permitted range.**

`SetRatioInside(double value)` is a private helper in `CompressionControls` that `Adjust` uses to commit a ratio change only when the input is acceptable. With cyclomatic complexity 2 and a `bool` result, the implementation pattern is a single validation branch: check whether `value` is inside allowed bounds, then update internal ratio state on success and return `true`. On failure, it returns `false` and avoids mutating state, letting the caller handle invalid interactive input without exceptions.


#### [[CompressionControls.SetRatioInside]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool SetRatioInside(double value)
```

**Called-by <-**
- [[CompressionControls.Adjust]]

