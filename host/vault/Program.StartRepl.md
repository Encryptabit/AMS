---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/error-handling
---
# Program::StartRepl
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Runs the AMS interactive REPL by reading user commands, handling built-ins, and executing root command operations within REPL state context until exit.**

`StartRepl` is an async interactive command loop that initializes a `ReplState`, prints startup help, and repeatedly prompts using `Prompt(state)` before reading from `Console.ReadLine()`. It normalizes input (`Trim`), ignores empty commands, exits on `IsExit(input)`, and short-circuits built-ins via `await TryHandleBuiltInAsync(...)` before dispatching non-built-ins. For dispatched commands it tokenizes input with `ParseInput` and executes in the current REPL scope using `await ExecuteWithScopeAsync(...)`. The loop has broad exception handling around command execution, printing both `ex.Message` and `ex.StackTrace`, then continuing.


#### [[Program.StartRepl]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task StartRepl(RootCommand rootCommand)
```

**Calls ->**
- [[Program.ExecuteWithScopeAsync]]
- [[Program.IsExit]]
- [[Program.ParseInput]]
- [[Program.Prompt]]
- [[Program.TryHandleBuiltInAsync]]

**Called-by <-**
- [[Program.Main]]

