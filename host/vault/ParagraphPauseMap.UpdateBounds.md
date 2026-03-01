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
# ParagraphPauseMap::UpdateBounds
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Validates and updates the paragraph’s current start/end bounds while enforcing a valid time range.**

`UpdateBounds` mutates the paragraph map’s current timing window after validating numeric input. It throws `ArgumentOutOfRangeException` when `start` or `end` is non-finite (`!double.IsFinite(...)`), and normalizes reversed bounds by setting `end = start` when `end < start`. It then assigns `CurrentStart` and `CurrentEnd`, guaranteeing a non-negative current duration.


#### [[ParagraphPauseMap.UpdateBounds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void UpdateBounds(double start, double end)
```

