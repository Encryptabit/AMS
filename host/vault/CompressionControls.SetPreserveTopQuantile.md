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
  - llm/error-handling
---
# CompressionControls::SetPreserveTopQuantile
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Validate and set the preserve-top-quantile compression option, returning whether the update was accepted.**

`SetPreserveTopQuantile(double value)` is a private state mutator in `CompressionControls` with a single-branch control path (complexity 2) that validates the incoming quantile before applying it. It returns a boolean status so callers can distinguish rejected input from a successful update, rather than throwing on routine invalid values. Because `Adjust` calls it, this method is the focused validation-and-commit gate for the top-quantile compression parameter in the interactive session state.


#### [[CompressionControls.SetPreserveTopQuantile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool SetPreserveTopQuantile(double value)
```

**Called-by <-**
- [[CompressionControls.Adjust]]

