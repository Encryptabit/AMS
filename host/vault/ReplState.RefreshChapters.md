---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 13
fan_in: 3
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# ReplState::RefreshChapters
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Persist and re-synchronize chapter selection when REPL state or working context changes.**

`RefreshChapters()` is a `ReplState` state-maintenance method that persists current session state via `PersistState` and then re-resolves the active chapter through `SelectChapterByIndexInternal` and `SelectChapterByNameInternal` fallback paths. Its cyclomatic complexity (13) indicates multiple guards/branches for selection validity and recovery rather than a linear update. Being called from `.ctor`, `CreatePrepRenameCommand`, and `SetWorkingDirectory` ties it to initialization and context-shift flows where chapter bindings can become stale.


#### [[ReplState.RefreshChapters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void RefreshChapters()
```

**Calls ->**
- [[ReplState.PersistState]]
- [[ReplState.SelectChapterByIndexInternal]]
- [[ReplState.SelectChapterByNameInternal]]

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[ReplState..ctor]]
- [[ReplState.SetWorkingDirectory]]

