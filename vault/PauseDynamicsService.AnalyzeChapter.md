---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 9
fan_in: 3
fan_out: 5
tags:
  - method
---
# PauseDynamicsService::AnalyzeChapter
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`


#### [[PauseDynamicsService.AnalyzeChapter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseAnalysisReport AnalyzeChapter(TranscriptIndex transcript, BookIndex bookIndex, HydratedTranscript hydrated, PausePolicy policy, IReadOnlyList<(double Start, double End)> intraSentenceSilences = null, bool includeAllIntraSentenceGaps = true)
```

**Calls ->**
- [[PauseClassSummary.FromDurations]]
- [[PauseDynamicsService.BuildHeadingParagraphSet]]
- [[PauseDynamicsService.BuildInterSentenceSpans]]
- [[PauseDynamicsService.BuildIntraSentenceSpans]]
- [[PauseDynamicsService.BuildSentenceParagraphMap]]

**Called-by <-**
- [[ValidateTimingSession.LoadSessionContextAsync]]
- [[PauseDynamicsService.Execute]]
- [[PauseMapBuilder.Build]]

