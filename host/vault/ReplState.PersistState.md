---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "private"
complexity: 4
fan_in: 5
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/error-handling
---
# ReplState::PersistState
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Persists the current REPL working directory and chapter-selection mode to disk so the next session can restore user context.**

`PersistState()` is a best-effort state snapshot writer that first returns immediately when `_suppressPersist` is set (to prevent writes during initialization/loading). It derives the parent folder from `_stateFilePath`, ensures it exists with `Directory.CreateDirectory`, builds a `PersistedReplState` from `WorkingDirectory`, `_lastSelectedChapterName`, and `RunAllChapters`, then serializes it with `JsonSerializer` (`WriteIndented = true`) and writes it with `File.WriteAllText`. Any exception during directory creation, serialization, or file IO is caught and downgraded to a console warning, so persistence failures do not break REPL flow.


#### [[ReplState.PersistState]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void PersistState()
```

**Called-by <-**
- [[ReplState..ctor]]
- [[ReplState.RefreshChapters]]
- [[ReplState.SelectChapterByIndexInternal]]
- [[ReplState.SetWorkingDirectory]]
- [[ReplState.UseAllChapters]]

