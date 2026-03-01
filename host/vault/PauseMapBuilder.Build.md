---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 16
fan_in: 2
fan_out: 14
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# PauseMapBuilder::Build
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.

## Summary
**Builds a hierarchical chapter pause map by analyzing pause spans and aggregating them across sentence, paragraph, and chapter scopes.**

`PauseMapBuilder.Build` validates inputs, applies a default policy (`PausePolicyPresets.House`) when none is provided, and derives sentence/paragraph topology from the hydrated transcript (`BuildSentenceToParagraphMap`, `BuildParagraphSentenceOrder`). It runs `PauseDynamicsService.AnalyzeChapter(...)` to get pause spans, then routes spans into sentence-, paragraph-, or chapter-level collectors: intra-sentence comma spans go to sentence collectors, same-paragraph inter-sentence spans go to paragraph collectors, and cross-paragraph spans go to the chapter collector. After collecting pauses it builds sentence maps, lets paragraph collectors absorb sentence-level duration stats, builds ordered paragraph maps, and lets the chapter collector absorb paragraph durations before building the final chapter map. The method returns the assembled `ChapterPauseMap` hierarchy with timeline/stats aggregated across all levels.


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

