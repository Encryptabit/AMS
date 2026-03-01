---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
---
# Program::HandleDirectoryCommandAsync
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Handle directory-related built-in REPL commands by either showing current state or updating the working directory based on parsed tokens.**

`HandleDirectoryCommandAsync` is an async REPL built-in handler that inspects `tokens` and applies directory-command behavior to `ReplState`. Its branching complexity (8) suggests multiple validation/dispatch paths, with read-style paths delegating to `PrintState` and mutation paths delegating to `SetWorkingDirectory`. It is part of internal command routing rather than startup flow, and is reached from `TryHandleBuiltInAsync`.


#### [[Program.HandleDirectoryCommandAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task HandleDirectoryCommandAsync(IReadOnlyList<string> tokens, ReplState state)
```

**Calls ->**
- [[ReplState.PrintState]]
- [[ReplState.SetWorkingDirectory]]

**Called-by <-**
- [[Program.TryHandleBuiltInAsync]]

