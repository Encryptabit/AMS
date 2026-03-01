---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# IPauseDynamicsService::AnalyzeChapter
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Defines the service boundary for generating pause analysis results from chapter transcript data and policy settings.**

In `IPauseDynamicsService`, `AnalyzeChapter` is an interface contract (no method body) that defines the chapter-level pause analysis API. It requires transcript/book context plus a pause policy, and optionally accepts hydrated transcript data, explicit intra-sentence silence spans, and a flag controlling whether all intra-sentence gaps are included. It returns a `PauseAnalysisReport`, with concrete analysis behavior supplied by implementing classes.


#### [[IPauseDynamicsService.AnalyzeChapter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
PauseAnalysisReport AnalyzeChapter(TranscriptIndex transcript, BookIndex bookIndex, HydratedTranscript hydrated, PausePolicy policy, IReadOnlyList<(double Start, double End)> intraSentenceSilences = null, bool includeAllIntraSentenceGaps = true)
```

