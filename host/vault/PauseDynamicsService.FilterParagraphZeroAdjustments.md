---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::FilterParagraphZeroAdjustments
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Removes non-heading pause adjustments that involve paragraph zero while keeping special chapter-boundary classes.**

`FilterParagraphZeroAdjustments` post-filters a transform plan to suppress adjustments that touch paragraph `0`, except for `ChapterHead` and `PostChapterRead` classes which are always retained. It short-circuits when there are no adjustments, uses a local `TouchesParagraphZero` lookup against `sentenceToParagraph` (ignoring negative sentence IDs), and rebuilds `PauseAdjusts` with LINQ filtering. If no items were removed it returns the original plan; otherwise it returns a new `PauseTransformSet` preserving existing `BreathCuts` with filtered adjustments.


#### [[PauseDynamicsService.FilterParagraphZeroAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PauseTransformSet FilterParagraphZeroAdjustments(PauseTransformSet plan, IReadOnlyDictionary<int, int> sentenceToParagraph)
```

**Calls ->**
- [[TouchesParagraphZero]]

**Called-by <-**
- [[PauseDynamicsService.Execute]]

