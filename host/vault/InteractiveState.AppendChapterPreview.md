---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::AppendChapterPreview
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Append the current chapter’s preview markup to the manuscript output buffer for the interactive timing-validation session.**

`AppendChapterPreview(StringBuilder sb)` is a private helper in `Ams.Cli.Commands.ValidateTimingSession.InteractiveState` that writes chapter-preview markup into a shared buffer during `BuildManuscriptMarkup`. It delegates paragraph-level formatting to `AppendParagraphMarkup`, so this method appears to own chapter-level flow while reusing a dedicated paragraph renderer. With cyclomatic complexity 5, the implementation likely contains several branch points for preview composition and state-dependent output.


#### [[InteractiveState.AppendChapterPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendChapterPreview(StringBuilder sb)
```

**Calls ->**
- [[InteractiveState.AppendParagraphMarkup]]

**Called-by <-**
- [[InteractiveState.BuildManuscriptMarkup]]

