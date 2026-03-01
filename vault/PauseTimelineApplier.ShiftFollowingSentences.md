---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
---
# PauseTimelineApplier::ShiftFollowingSentences
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`


#### [[PauseTimelineApplier.ShiftFollowingSentences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ShiftFollowingSentences(IDictionary<int, SentenceTiming> timeline, IReadOnlyList<int> orderedSentenceIds, IReadOnlyDictionary<int, int> indexBySentence, int pivotIndex, double delta)
```

**Called-by <-**
- [[PauseTimelineApplier.Apply]]
- [[PauseTimelineApplier.ApplyChapterHeadAdjust]]
- [[PauseTimelineApplier.ApplyInterSentenceAdjust]]

