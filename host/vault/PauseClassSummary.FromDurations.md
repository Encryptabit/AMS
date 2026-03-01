---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseAnalysisReport.cs"
access_modifier: "public"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseClassSummary::FromDurations
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseAnalysisReport.cs`

## Summary
**Computes descriptive summary metrics for a pause class from raw duration samples.**

`FromDurations` builds class-level pause statistics from a duration sequence after validating `durations` is non-null. It filters to finite non-negative values, returning the predefined `Empty` summary when nothing remains. For valid data it sorts values, computes total/min/max/mean, and derives median with odd/even branch logic. It returns a populated `PauseClassSummary` record with these aggregates.


#### [[PauseClassSummary.FromDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static PauseClassSummary FromDurations(IEnumerable<double> durations)
```

**Called-by <-**
- [[PauseDynamicsService.AnalyzeChapter]]
- [[PauseDynamicsServiceTests.PlanTransforms_CompressesSentencePauseOutsideWindow]]
- [[PauseDynamicsServiceTests.PlanTransforms_PreservesTopQuantileForLongestGap]]

