---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# PauseWindow::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs`

## Summary
**Creates an immutable pause-duration window with strict numeric validity and monotonic bound checks.**

The `PauseWindow` constructor validates both bounds before assignment, rejecting `NaN` via `double.IsNaN(...)` and infinities via `double.IsInfinity(...)` with `ArgumentException`s. It also enforces ordering by throwing when `max < min` rather than normalizing inputs. After passing validation, it assigns immutable `Min` and `Max` properties.


#### [[PauseWindow..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseWindow(double min, double max)
```

