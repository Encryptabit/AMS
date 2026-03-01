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
# CompressionControls::ToPolicy
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build the runtime PausePolicy for preview rebuilding by applying CompressionControls over a supplied base policy.**

ToPolicy is a low-complexity adapter that composes an effective PausePolicy from basePolicy and the current CompressionControls state. The implementation is effectively a direct property projection/override with no control-flow branches (complexity 1), and it is invoked by RebuildPreview so preview generation always uses the current interactive compression settings.


#### [[CompressionControls.ToPolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PausePolicy ToPolicy(PausePolicy basePolicy)
```

**Called-by <-**
- [[CompressionState.RebuildPreview]]

