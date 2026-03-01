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
  - llm/utility
---
# PipelineCommand::CreatePrepCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Create and wire the `prep` CLI command group by aggregating its stage, rename, and reset subcommands.**

`CreatePrepCommand` builds a `System.CommandLine.Command` node for the `"prep"` verb with description `"Preparation utilities for batch handoff"`. The implementation is a thin composition factory: it instantiates the parent command, attaches three subcommands via `AddCommand(CreatePrepStageCommand())`, `AddCommand(CreatePrepRenameCommand())`, and `AddCommand(CreatePrepResetCommand())`, then returns the configured command instance to the caller (`Create`).


#### [[PipelineCommand.CreatePrepCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePrepCommand()
```

**Calls ->**
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.CreatePrepResetCommand]]
- [[PipelineCommand.CreatePrepStageCommand]]

**Called-by <-**
- [[PipelineCommand.Create]]

