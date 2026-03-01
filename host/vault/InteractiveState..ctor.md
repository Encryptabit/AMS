---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 9
fan_in: 0
fan_out: 4
tags:
  - method
  - llm/factory
  - llm/di
  - llm/validation
  - llm/utility
---
# InteractiveState::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Initializes interactive timing-validation state by deriving timeline, entry, visibility, and lookup structures from pause analysis and sentence/paragraph mappings.**

This constructor composes `InteractiveState` from chapter pause inputs (`ChapterPauseMap`, `PauseAnalysisReport`, `PausePolicy`) plus sentence/paragraph lookup maps, then runs a deterministic initialization pipeline. It builds the baseline timeline via `BuildBaselineTimeline`, materializes interactive entries with `BuildEntries`, enforces visible tree state through `EnsureTreeVisibility`, and precomputes pause lookup indexes with `PopulatePauseLookups`. With complexity 9, it appears to be orchestration-heavy initialization with conditional state shaping rather than algorithmically intensive processing.


#### [[InteractiveState..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public InteractiveState(ChapterPauseMap chapter, PauseAnalysisReport analysis, PausePolicy basePolicy, IReadOnlyDictionary<int, string> sentenceLookup, IReadOnlyList<ValidateTimingSession.ParagraphInfo> paragraphs, IReadOnlyDictionary<int, int> sentenceToParagraph, IReadOnlyDictionary<int, IReadOnlyList<int>> paragraphSentences)
```

**Calls ->**
- [[InteractiveState.BuildBaselineTimeline]]
- [[InteractiveState.BuildEntries]]
- [[InteractiveState.EnsureTreeVisibility]]
- [[InteractiveState.PopulatePauseLookups]]

