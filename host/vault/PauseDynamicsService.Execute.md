---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::Execute
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Runs end-to-end pause analysis, transform planning/filtering, and timeline application for a chapter.**

`Execute` orchestrates the full pause-dynamics pipeline by running `AnalyzeChapter`, planning adjustments with `PlanTransforms`, and optionally post-filtering zeroed paragraph-level changes using `FilterParagraphZeroAdjustments` with a sentence→paragraph map from `BuildSentenceParagraphMap`. It then applies the final transform set to baseline transcript timings via `Apply` and returns a composed `PauseDynamicsResult` containing analysis, plan, and apply outputs. The method itself is a thin coordinator; transformation logic lives in the delegated helpers.


#### [[PauseDynamicsService.Execute]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseDynamicsResult Execute(TranscriptIndex transcript, BookIndex bookIndex, HydratedTranscript hydrated, PausePolicy policy, IReadOnlyList<(double Start, double End)> intraSentenceSilences = null, bool includeAllIntraSentenceGaps = true)
```

**Calls ->**
- [[PauseDynamicsService.AnalyzeChapter]]
- [[PauseDynamicsService.Apply]]
- [[PauseDynamicsService.BuildSentenceParagraphMap]]
- [[PauseDynamicsService.FilterParagraphZeroAdjustments]]
- [[PauseDynamicsService.PlanTransforms]]

