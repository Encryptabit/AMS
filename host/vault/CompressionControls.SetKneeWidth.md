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
  - llm/utility
---
# CompressionControls::SetKneeWidth
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Validate a proposed knee-width value and update compression control state when the value is acceptable.**

`SetKneeWidth(double value)` is a private state-mutation helper in `CompressionControls` invoked by `Adjust` to apply a knee-width change during the interactive validation session. With cyclomatic complexity 2, it follows a single-branch pattern: validate the incoming `double`, return `false` on invalid input, otherwise commit the new knee width and return `true`. The boolean result is used as control flow for interactive command handling instead of propagating exceptions.


#### [[CompressionControls.SetKneeWidth]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool SetKneeWidth(double value)
```

**Called-by <-**
- [[CompressionControls.Adjust]]

