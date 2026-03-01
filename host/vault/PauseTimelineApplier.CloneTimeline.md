---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# PauseTimelineApplier::CloneTimeline
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`

## Summary
**Builds an independent copy of the baseline sentence timeline for safe in-place transformation.**

`CloneTimeline` creates a deep copy of sentence timing entries from `baseline` into a new `Dictionary<int, SentenceTiming>` pre-sized to `baseline.Count`. It iterates each key/value pair and constructs a fresh `SentenceTiming` instance using the source `StartSec`, `EndSec`, `FragmentBacked`, and `Confidence`. This avoids mutating caller-provided timeline objects during later adjustment passes.


#### [[PauseTimelineApplier.CloneTimeline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, SentenceTiming> CloneTimeline(IReadOnlyDictionary<int, SentenceTiming> baseline)
```

**Called-by <-**
- [[PauseTimelineApplier.Apply]]

