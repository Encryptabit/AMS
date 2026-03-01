---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseStats::FromDurations
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Creates a normalized statistical summary of pause durations from raw samples, ignoring invalid values.**

`FromDurations` guards against null input (`ArgumentNullException`), then filters durations to non-negative finite values, sorts them, and materializes to a list. If no valid values remain, it returns the singleton `PauseStats.Empty`. Otherwise it computes `Total` (`Sum`), `Min`/`Max` from list endpoints, median via odd/even index logic, and `Mean` as `total / count`, then returns a new `PauseStats` record with those aggregates.


#### [[PauseStats.FromDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseStats FromDurations(IEnumerable<double> durations)
```

**Called-by <-**
- [[PauseStatsSet.FromDurations]]

