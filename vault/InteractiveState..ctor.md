---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 9
fan_in: 0
fan_out: 4
tags:
  - method
---
# InteractiveState::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

