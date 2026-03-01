---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 14
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# Program::TryHandleBuiltInAsync
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Handle and execute built-in REPL commands against current state, returning whether the input was consumed.**

`TryHandleBuiltInAsync` is the REPL’s built-in command dispatcher called from `StartRepl`, with multiple branches (complexity 14) to route parsed user input. It parses tokens via `ParseInput`, short-circuits recognized built-ins to dedicated handlers (`HandleDirectoryCommandAsync`, `HandleModeCommand`, `HandleUseCommand`, `ListChapters`, `PrintState`), and uses `ExecuteWithScopeAsync` when command execution needs the scoped command pipeline with `RootCommand`. Its `Task<bool>` result signals whether the input was handled internally versus needing normal command processing.


#### [[Program.TryHandleBuiltInAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<bool> TryHandleBuiltInAsync(string input, ReplState state, RootCommand rootCommand)
```

**Calls ->**
- [[Program.ExecuteWithScopeAsync]]
- [[Program.HandleDirectoryCommandAsync]]
- [[Program.HandleModeCommand]]
- [[Program.HandleUseCommand]]
- [[Program.ParseInput]]
- [[ReplState.ListChapters]]
- [[ReplState.PrintState]]

**Called-by <-**
- [[Program.StartRepl]]

