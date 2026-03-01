---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# ReplState::SetWorkingDirectory
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Set the REPL’s working directory and immediately realign persisted state plus chapter context to that directory.**

`SetWorkingDirectory(string path)` updates `ReplState` to point at a new working path, then executes side effects in order: `PersistState`, `RefreshChapters`, and `SelectChapterByIndexInternal`. This keeps persisted REPL state and in-memory chapter navigation synchronized immediately after a directory change. With complexity 4, the implementation is likely a small control-flow wrapper around path/state updates and chapter reselection logic.


#### [[ReplState.SetWorkingDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetWorkingDirectory(string path)
```

**Calls ->**
- [[ReplState.PersistState]]
- [[ReplState.RefreshChapters]]
- [[ReplState.SelectChapterByIndexInternal]]

**Called-by <-**
- [[Program.HandleDirectoryCommandAsync]]

