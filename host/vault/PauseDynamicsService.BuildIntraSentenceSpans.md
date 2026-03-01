---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 20
fan_in: 1
fan_out: 5
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::BuildIntraSentenceSpans
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.

## Summary
**Transforms intra-sentence silence intervals into punctuation-aware comma pause spans for downstream pause compression.**

`BuildIntraSentenceSpans` yields comma-class pause spans from detected silences that fall inside reliable, non-heading sentence bounds. It first orders valid-timing sentences, exits early on empty sentences/silences, and if `includeAllIntraSentenceGaps` is true delegates directly to `BuildAllIntraSentenceSpans(...)`. Otherwise it filters silences for finite/min-duration windows, maps each to a containing sentence (with tolerance), applies edge guards, groups by sentence, then for each eligible sentence resolves punctuation anchors (`GetPunctuationTimes`) and selects punctuation-aligned gaps via `MatchSilencesToPunctuation`. Each selected gap is emitted as `PauseSpan` with `PauseClass.Comma`, `HasGapHint: true`, no paragraph/chapter crossing flags, and debug logging around punctuation-missing and selected-gap cases.


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

