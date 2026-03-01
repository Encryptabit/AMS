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
# PauseInterval::SetCurrent
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Validates and sets the interval’s current start/end timing values while enforcing a non-negative range.**

`SetCurrent` updates the mutable timing window of a `PauseInterval` with strict numeric guards. It throws `ArgumentOutOfRangeException` if either `start` or `end` is non-finite (`!double.IsFinite(...)`), and normalizes inverted input by clamping `end` to `start` when `end < start`. After validation/normalization, it assigns `CurrentStart` and `CurrentEnd`, ensuring `CurrentDuration` remains non-negative.


#### [[PauseInterval.SetCurrent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetCurrent(double start, double end)
```

