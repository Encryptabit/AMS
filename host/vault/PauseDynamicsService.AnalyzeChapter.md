---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 9
fan_in: 3
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::AnalyzeChapter
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Analyzes chapter timing data into categorized pause spans and per-class pause statistics for downstream transform planning.**

`AnalyzeChapter` validates required inputs, builds sentence↔paragraph and heading mappings (`BuildSentenceParagraphMap`, `BuildHeadingParagraphSet`), and seeds pause spans from inter-sentence gaps via `BuildInterSentenceSpans`. If `intraSentenceSilences` is provided, it optionally augments spans using `BuildIntraSentenceSpans` with hydrated sentence context and heading-aware filtering. It then groups spans by `PauseClass`, computes per-class stats with `PauseClassSummary.FromDurations`, ensures a `PauseClass.Comma` entry exists (defaulting to `PauseClassSummary.Empty`), and returns a `PauseAnalysisReport` containing spans plus class summaries.


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

