---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 20
fan_in: 1
fan_out: 5
tags:
  - method
  - danger/high-complexity
---
# PauseDynamicsService::BuildIntraSentenceSpans
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.


#### [[PauseDynamicsService.BuildIntraSentenceSpans]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<PauseSpan> BuildIntraSentenceSpans(TranscriptIndex transcript, BookIndex bookIndex, Dictionary<int, HydratedSentence> hydratedSentenceMap, IReadOnlyList<(double Start, double End)> silences, HashSet<int> headingSentenceIds, bool includeAllIntraSentenceGaps)
```

**Calls ->**
- [[Log.Debug]]
- [[BuildAllIntraSentenceSpans]]
- [[PauseDynamicsService.GetPunctuationTimes]]
- [[PauseDynamicsService.IsReliableSentence]]
- [[PauseDynamicsService.MatchSilencesToPunctuation]]

**Called-by <-**
- [[PauseDynamicsService.AnalyzeChapter]]

