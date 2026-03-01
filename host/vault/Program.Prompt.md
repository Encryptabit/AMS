---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# Program::Prompt
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Constructs the console prompt text that reflects current REPL directory/scope context and ASR process status.**

`Prompt(ReplState state)` is a pure formatting helper for the REPL loop: it reads `state.WorkingDirectoryLabel`, `state.ScopeLabel`, and `AsrProcessSupervisor.StatusLabel`, then returns an interpolated prompt string formatted as `[AMS|{dirLabel}|{scopeLabel}|{asrLabel}]> `. The method has no branching, validation, or side effects.


#### [[Program.Prompt]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string Prompt(ReplState state)
```

**Called-by <-**
- [[Program.StartRepl]]

