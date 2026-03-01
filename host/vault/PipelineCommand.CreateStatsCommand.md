---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/error-handling
---
# PipelineCommand::CreateStatsCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Creates and configures the pipeline `stats` command, including option parsing, execution dispatch to `RunStats`, and failure/cancellation handling.**

`CreateStatsCommand` constructs the `stats` CLI subcommand using `System.CommandLine`, defining `--work-dir`, `--book-index`, `--chapter`, and `--all` options with nullable/default delegates. The handler reads parsed option values and the command cancellation token from `InvocationContext`, then calls `RunStats(workDir, bookIndex, chapter, all, cancellationToken)`. It catches `OperationCanceledException` and general `Exception`, logs via `Log.Debug`/`Log.Error`, and sets `context.ExitCode = 1` on both error paths before returning the configured `Command`.


#### [[PipelineCommand.CreateStatsCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateStatsCommand()
```

**Calls ->**
- [[PipelineCommand.RunStats]]
- [[Log.Debug]]
- [[Log.Error]]

**Called-by <-**
- [[PipelineCommand.Create]]

