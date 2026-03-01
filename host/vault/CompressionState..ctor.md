---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# CompressionState::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Instantiate compression-session state from interactive inputs and immediately rebuild the preview representation.**

This constructor initializes `CompressionState` by taking `scope`, `controls`, `pauses`, and `basePolicy` as inputs and then invoking `RebuildPreview` to compute the initial derived preview state. Given cyclomatic complexity 2, the implementation is lightweight and likely limited to field/property assignment plus a single guard/branch around preview setup. The key behavior is eager preview synchronization at construction time rather than deferred computation.


#### [[CompressionState..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public CompressionState(ValidateTimingSession.ScopeEntry scope, ValidateTimingSession.InteractiveState.CompressionControls controls, List<ValidateTimingSession.EditablePause> pauses, PausePolicy basePolicy)
```

**Calls ->**
- [[CompressionState.RebuildPreview]]

