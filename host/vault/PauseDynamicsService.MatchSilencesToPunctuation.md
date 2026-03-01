---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 29
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PauseDynamicsService::MatchSilencesToPunctuation
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

> [!danger] High Complexity (29)
> Cyclomatic complexity: 29. Consider refactoring into smaller methods.

## Summary
**Selects the best ordered set of silence intervals that correspond to sentence punctuation timing anchors.**

`MatchSilencesToPunctuation` aligns punctuation anchor times to candidate silence gaps using a monotonic dynamic-programming assignment. It returns empty when either input set is empty, builds optional word-center priors (`BuildWordCenters`), then fills `dp`/`prev` matrices where each match cost is `DistanceToInterval(punctuationTime, gap)` plus a small late-gap penalty when a gap starts after the corresponding word center. It picks the best reachable terminal state (allowing partial punctuation coverage), backtracks assigned gap indices, and emits mapped gaps as `(Start, End, PauseProvenance.ScriptAndTextGrid)`; mapping failures or backtrack inconsistencies are logged and return empty results.


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

