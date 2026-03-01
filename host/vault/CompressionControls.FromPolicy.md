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
  - llm/factory
---
# CompressionControls::FromPolicy
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Create a `CompressionControls` instance from `PausePolicy` for the validation session’s interactive compression logic.**

`FromPolicy` is a static, low-complexity mapper on `InteractiveState.CompressionControls` that constructs control state directly from a `PausePolicy`, likely as a single return expression/object initializer or simple switch without iteration. The method is deterministic and side-effect free, centralizing policy-to-controls translation in one place. It is used by `EnsureCompressionStateForCurrentScope` to derive compression controls before enforcing scope-specific compression behavior.


#### [[CompressionControls.FromPolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ValidateTimingSession.InteractiveState.CompressionControls FromPolicy(PausePolicy policy)
```

**Called-by <-**
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

