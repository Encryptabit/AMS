---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# PauseTimelineApplier::ApplyInterSentenceAdjust
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`


#### [[PauseTimelineApplier.ApplyInterSentenceAdjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ApplyInterSentenceAdjust(IDictionary<int, SentenceTiming> timeline, IReadOnlyList<int> orderedSentenceIds, IReadOnlyDictionary<int, int> indexBySentence, int leftId, int rightId, double targetDuration)
```

**Calls ->**
- [[PauseTimelineApplier.ShiftFollowingSentences]]

**Called-by <-**
- [[PauseTimelineApplier.Apply]]

