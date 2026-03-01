---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 1
tags:
  - method
---
# PauseDynamicsService::BuildInterSentenceSpans
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.BuildInterSentenceSpans]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<PauseSpan> BuildInterSentenceSpans(TranscriptIndex transcript, IReadOnlyDictionary<int, int> sentenceToParagraph, HashSet<int> headingParagraphIds)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PauseDynamicsService.AnalyzeChapter]]

