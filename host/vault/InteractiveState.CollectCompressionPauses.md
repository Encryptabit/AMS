---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 19
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/data-access
  - llm/validation
---
# InteractiveState::CollectCompressionPauses
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (19)
> Cyclomatic complexity: 19. Consider refactoring into smaller methods.

## Summary
**Collect the exact set of pauses affected by the current navigation scope so compression state/preview can be built from a stable, sorted pause list.**

`CollectCompressionPauses` assembles a deduplicated `List<EditablePause>` for a given `ScopeEntry` using a `HashSet<EditablePause>` and a local `AddPause` null guard. It special-cases `ScopeEntryKind.Pause` by returning only `scope.Pause`, then branches by scope kind: `Chapter` unions `_chapterPauses` with all `_sentencePauses` values, `Paragraph` walks `GetParagraphRange(scope.ParagraphId.Value)` and pulls sentence pauses via `GetParagraphSentenceIds` plus chapter pauses whose `LeftParagraphId` matches, and `Sentence` takes pauses from the current sentence index to the end of that paragraph. Invalid/unhandled scope shapes return an empty list, and all successful paths are ordered by `pause.Span.StartSec` before returning.


#### [[InteractiveState.CollectCompressionPauses]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ValidateTimingSession.EditablePause> CollectCompressionPauses(ValidateTimingSession.ScopeEntry scope)
```

**Calls ->**
- [[AddPause]]
- [[InteractiveState.GetParagraphRange]]
- [[InteractiveState.GetParagraphSentenceIds]]

**Called-by <-**
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

