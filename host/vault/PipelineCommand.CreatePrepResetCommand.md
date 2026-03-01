---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# PipelineCommand::CreatePrepResetCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Defines the prep reset CLI command that validates the root directory and runs either hard or soft artifact cleanup.**

Creates and returns a System.CommandLine `reset` subcommand for `prep`, wiring `--root/-r` (optional `DirectoryInfo`, default `null`) and `--hard` (bool, default `false`). Its handler gets the invocation cancellation token, resolves the target directory via `CommandInputResolver.ResolveDirectory(...)`, refreshes it, and throws `DirectoryNotFoundException` if the path does not exist. It then branches on `--hard`, logs with `Log.Debug(...)`, and calls `PerformHardReset(root, cancellationToken)` or `PerformSoftReset(root, cancellationToken)`.


#### [[PipelineCommand.CreatePrepResetCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePrepResetCommand()
```

**Calls ->**
- [[PipelineCommand.PerformHardReset]]
- [[PipelineCommand.PerformSoftReset]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreatePrepCommand]]

