---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateListParamsCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates and returns the CLI command that runs Plugalyzer to list available parameters for a specified plugin path.**

CreateListParamsCommand builds a `System.CommandLine.Command` named `list-params` with a required `--plugin` (`-p`) option plus optional `--sample-rate` and `--block-size` options. Its async handler reads parsed values, resolves the plugin path from `Directory.GetCurrentDirectory()` via `ResolvePath`, and composes `PlugalyzerService` arguments beginning with `listParameters` and `--plugin=...`, appending sample/block arguments when provided. It then calls `PlugalyzerService.RunAsync` with console output/error delegates, and on failure catches `Exception`, logs with `Log.Error`, and sets `context.ExitCode = 1`.


#### [[DspCommand.CreateListParamsCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateListParamsCommand()
```

**Calls ->**
- [[DspCommand.ResolvePath]]
- [[PlugalyzerService.RunAsync]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

