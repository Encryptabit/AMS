---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
---
# InteractiveState::GetChapterEntry
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return the current chapter scope entry from interactive timing-session state for options panel construction.**

In `InteractiveState`, `GetChapterEntry()` is a zero-branch accessor (complexity 1) that returns a `ValidateTimingSession.ScopeEntry` representing the currently selected chapter scope stored in session state. It performs no control flow, validation, or mutation, and is used by `BuildOptionsPanel` as a read path when assembling interactive options.


#### [[InteractiveState.GetChapterEntry]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.ScopeEntry GetChapterEntry()
```

**Called-by <-**
- [[TimingRenderer.BuildOptionsPanel]]

