---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# PausePolicy::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseModels.cs`

## Summary
**Builds a configurable pause-processing policy by wiring required pause windows and compression/dynamics parameters.**

This constructor initializes all pause-policy configuration fields directly from arguments, including per-class `PauseWindow`s and compression tuning parameters (`KneeWidth`, `RatioInside`, `RatioOutside`, `PreserveTopQuantile`). It enforces required object dependencies by null-checking `comma`, `sentence`, and `paragraph` and throwing `ArgumentNullException` when missing. Scalar `double` parameters are assigned without additional range validation or normalization.


#### [[PausePolicy..ctor_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PausePolicy(PauseWindow comma, PauseWindow sentence, PauseWindow paragraph, double headOfChapter, double postChapterRead, double tail, double kneeWidth = 0.08, double ratioInside = 1.25, double ratioOutside = 3, double preserveTopQuantile = 0.95)
```

