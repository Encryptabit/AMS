---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PauseDynamicsService::CloneBaseline
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Copies a sentence-timing dictionary into a new mutable dictionary with recreated timing values.**

`CloneBaseline` creates a deep-enough copy of the baseline timing map by allocating a new `Dictionary<int, SentenceTiming>` sized to `baseline.Count` and rehydrating each entry into a new `SentenceTiming` with copied scalar fields (`StartSec`, `EndSec`, `FragmentBacked`, `Confidence`). This avoids sharing original value instances when returning pass-through timelines.


#### [[PauseDynamicsService.CloneBaseline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, SentenceTiming> CloneBaseline(IReadOnlyDictionary<int, SentenceTiming> baseline)
```

**Called-by <-**
- [[PauseDynamicsService.Apply]]

