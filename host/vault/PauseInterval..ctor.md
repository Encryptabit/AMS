---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseInterval::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Creates a validated pause interval that stores original timing/class metadata and initializes current timing state.**

The constructor validates `originalStart` and `originalEnd` with `double.IsFinite`, throwing `ArgumentOutOfRangeException` for non-finite inputs. It normalizes inverted ranges by clamping `originalEnd` up to `originalStart` when `originalEnd < originalStart`, guaranteeing a non-negative duration. It then sets immutable metadata (`Class`, `OriginalStart`, `OriginalEnd`, `HasHint`) and initializes mutable current timings (`CurrentStart`, `CurrentEnd`) to the original bounds.


#### [[PauseInterval..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseInterval(PauseClass pauseClass, double originalStart, double originalEnd, bool hasHint)
```

