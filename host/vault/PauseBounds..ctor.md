---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# PauseBounds::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Creates a pause-bounds value object holding raw minimum and maximum duration limits.**

`PauseBounds(double min, double max)` is a minimal value constructor that assigns the provided arguments directly to immutable `Min` and `Max` properties. It performs no validation, normalization, or ordering (for example, `min > max` is allowed and handled later by consumers such as `ComputeTargetDuration`).


#### [[PauseBounds..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseBounds(double min, double max)
```

