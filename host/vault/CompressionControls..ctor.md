---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# CompressionControls::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Initializes a `CompressionControls` object with compression-curve parameters for interactive timing-session validation behavior.**

This private constructor on `InteractiveState.CompressionControls` is an O(1) initializer that accepts four tuning parameters (`ratioInside`, `ratioOutside`, `kneeWidth`, `preserveTopQuantile`) and sets up a compression-control instance. The implementation is straight-line setup logic (no branching, async flow, or I/O), consistent with a small parameter object used by the validate-timing interactive command path.


#### [[CompressionControls..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private CompressionControls(double ratioInside, double ratioOutside, double kneeWidth, double preserveTopQuantile)
```

