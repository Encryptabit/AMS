---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 16
fan_in: 2
fan_out: 14
tags:
  - method
  - danger/high-complexity
---
# PauseMapBuilder::Build
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[PauseMapBuilder.Build]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ChapterPauseMap Build(TranscriptIndex transcript, BookIndex bookIndex, HydratedTranscript hydrated, PausePolicy policy = null, IReadOnlyList<(double Start, double End)> intraSentenceSilences = null, bool includeAllIntraSentenceGaps = false)
```

**Calls ->**
- [[PauseDynamicsService.AnalyzeChapter]]
- [[PauseMapBuilder.BuildParagraphSentenceOrder]]
- [[PauseMapBuilder.BuildSentenceToParagraphMap]]
- [[ChapterCollector.AbsorbDurations]]
- [[ChapterCollector.AddPause]]
- [[ChapterCollector.Build]]
- [[PauseMapBuilder.CreateParagraphCollectors]]
- [[PauseMapBuilder.CreateSentenceCollectors]]
- [[ParagraphCollector.AbsorbSentenceDurations]]
- [[ParagraphCollector.AddPause]]
- [[ParagraphCollector.Build]]
- [[SentenceCollector.AddPause]]
- [[SentenceCollector.Build]]
- [[PausePolicyPresets.House]]

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]
- [[ValidateTimingSession.LoadSessionContextAsync]]

