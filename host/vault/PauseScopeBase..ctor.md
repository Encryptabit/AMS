---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "protected"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# PauseScopeBase::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Initializes the base pause-scope object with required aggregated statistics.**

The `PauseScopeBase` constructor enforces a non-null `PauseStatsSet` dependency and stores it in the read-only `Stats` property. It uses a null-coalescing throw expression (`stats ?? throw new ArgumentNullException(nameof(stats))`) for immediate argument validation. No additional initialization logic is performed.


#### [[PauseScopeBase..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
protected PauseScopeBase(PauseStatsSet stats)
```

