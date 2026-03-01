---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAnalysisReport.cs"
access_modifier: "public"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
---
# PauseClassSummary::FromDurations
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseAnalysisReport.cs`


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

