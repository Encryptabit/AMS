---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# PauseTimelineApplier::ApplyChapterHeadAdjust
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseTimelineApplier.cs`


#### [[PauseTimelineApplier.ApplyChapterHeadAdjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ApplyChapterHeadAdjust(IDictionary<int, SentenceTiming> timeline, IReadOnlyList<int> orderedSentenceIds, IReadOnlyDictionary<int, int> indexBySentence, PauseAdjust adjust)
```

**Calls ->**
- [[PauseTimelineApplier.ShiftFollowingSentences]]

**Called-by <-**
- [[PauseTimelineApplier.Apply]]

