---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# Program::HandleUseCommand
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Parses and executes chapter scope selection for the REPL `use`/`mode` command against the current `ReplState`.**

`HandleUseCommand` performs branch-based parsing of REPL `use` arguments and updates `ReplState` accordingly. It first validates arity (`tokens.Count < 2`), then routes `"all"` to `UseAllChapters`, numeric values (via `int.TryParse`) to `UseChapterByIndex`, and all other input to `UseChapterByName` using `string.Join(' ', tokens.Skip(1))`. It emits targeted error messages for invalid index or missing chapter and calls `state.PrintState()` after each selection attempt to reflect the resulting scope.


#### [[Program.HandleUseCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void HandleUseCommand(IReadOnlyList<string> tokens, ReplState state)
```

**Calls ->**
- [[ReplState.PrintState]]
- [[ReplState.UseAllChapters]]
- [[ReplState.UseChapterByIndex]]
- [[ReplState.UseChapterByName]]

**Called-by <-**
- [[Program.HandleModeCommand]]
- [[Program.TryHandleBuiltInAsync]]

