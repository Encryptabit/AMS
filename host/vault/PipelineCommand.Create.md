---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/di
  - llm/validation
---
# PipelineCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Build and return the top-level `pipeline` CLI command with all pipeline-related subcommands and required service dependency.**

`Create(PipelineService pipelineService)` is a command-factory entry method that first validates its dependency via `ArgumentNullException.ThrowIfNull(pipelineService)`. It instantiates a `System.CommandLine.Command` named `"pipeline"` with the end-to-end pipeline description, then wires in four subcommands by calling `CreateRun(pipelineService)`, `CreatePrepCommand()`, `CreateVerifyCommand()`, and `CreateStatsCommand()` before returning the assembled command tree.


#### [[PipelineCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(PipelineService pipelineService)
```

**Calls ->**
- [[PipelineCommand.CreatePrepCommand]]
- [[PipelineCommand.CreateRun]]
- [[PipelineCommand.CreateStatsCommand]]
- [[PipelineCommand.CreateVerifyCommand]]

**Called-by <-**
- [[Program.Main]]

