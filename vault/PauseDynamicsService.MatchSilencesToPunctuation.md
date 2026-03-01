---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 29
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
---
# PauseDynamicsService::MatchSilencesToPunctuation
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

> [!danger] High Complexity (29)
> Cyclomatic complexity: 29. Consider refactoring into smaller methods.


#### [[PauseDynamicsService.MatchSilencesToPunctuation]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<(double Start, double End, PauseProvenance Provenance)> MatchSilencesToPunctuation(List<(double Start, double End)> gaps, IReadOnlyList<double> punctuationTimes, SentenceAlign sentence, BookIndex bookIndex)
```

**Calls ->**
- [[Log.Debug]]
- [[PauseDynamicsService.BuildWordCenters]]
- [[PauseDynamicsService.DistanceToInterval]]

**Called-by <-**
- [[PauseDynamicsService.BuildIntraSentenceSpans]]

