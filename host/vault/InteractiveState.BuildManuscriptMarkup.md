---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 15
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# InteractiveState::BuildManuscriptMarkup
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.

## Summary
**Build the manuscript-formatted text for one validation scope entry, including chapter preview, paragraph markup, and pause-sentence fallback content.**

Within `ValidateTimingSession.InteractiveState`, `BuildManuscriptMarkup(ValidateTimingSession.ScopeEntry entry)` assembles the per-scope manuscript output by sequencing `AppendChapterPreview`, paragraph rendering through `AppendParagraphMarkup`, and `AppendPauseSentencesFallback` when paragraph markup is unavailable or insufficient. Its complexity (15) indicates non-trivial branching over entry state while constructing the final string. It serves as the core formatter consumed by `BuildManuscript`.


#### [[InteractiveState.BuildManuscriptMarkup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string BuildManuscriptMarkup(ValidateTimingSession.ScopeEntry entry)
```

**Calls ->**
- [[InteractiveState.AppendChapterPreview]]
- [[InteractiveState.AppendParagraphMarkup]]
- [[InteractiveState.AppendPauseSentencesFallback]]

**Called-by <-**
- [[TimingRenderer.BuildManuscript]]

