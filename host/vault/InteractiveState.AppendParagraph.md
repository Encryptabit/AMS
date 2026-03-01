---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::AppendParagraph
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Append paragraph-scoped validation output by deriving paragraph metadata and delegating sentence entry construction.**

`AppendParagraph` is a thin orchestration method that mutates the provided `entries` collection for a single `ParagraphPauseMap` input. It computes paragraph-level metadata via `BuildParagraphLabel` and `GetParagraphInfo`, then delegates sentence-level entry creation to `AppendSentence` instead of implementing that logic inline. The reported complexity of 2 indicates only minimal control flow (typically a single guard/branch) around this pipeline.


#### [[InteractiveState.AppendParagraph]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendParagraph(List<ValidateTimingSession.ScopeEntry> entries, ParagraphPauseMap paragraph)
```

**Calls ->**
- [[InteractiveState.AppendSentence]]
- [[InteractiveState.BuildParagraphLabel]]
- [[InteractiveState.GetParagraphInfo]]

**Called-by <-**
- [[InteractiveState.BuildEntries]]

