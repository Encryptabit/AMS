---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 9
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# ValidateCommand::CreateTimingCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Creates and configures the validate/timing command, including async execution, context/artifact resolution, and explicit error reporting for invalid runtime state.**

`CreateTimingCommand()` is a low-complexity command factory that builds the `timing` CLI command, adds its init subcommand via `CreateTimingInitCommand`, and wires execution through an async `RunAsync` handler. The handler follows a resolve-then-run flow: it emits `Debug` output, resolves context with `ResolveWorkspace` and `ResolveBookIndex`, then selects artifacts via `TryResolveChapterArtifact`/`TryResolveAdjustedArtifact`. Invalid or missing prerequisites are handled with explicit `Error` calls rather than deep branching.


#### [[ValidateCommand.CreateTimingCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateTimingCommand()
```

**Calls ->**
- [[ValidateCommand.CreateTimingInitCommand]]
- [[ValidateCommand.TryResolveAdjustedArtifact]]
- [[ValidateTimingSession.RunAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[CommandInputResolver.TryResolveChapterArtifact]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]

**Called-by <-**
- [[ValidateCommand.Create]]

