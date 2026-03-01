---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/entry-point
---
# DspCommand::CreateFiltersCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the CLI subcommand that prints the available built-in FFmpeg filter helpers.**

`CreateFiltersCommand` builds a `System.CommandLine` `Command` named `filters` with the description “List built-in FFmpeg filter helpers.” It wires a synchronous `SetHandler` delegate that emits a header with `Log.Info("Built-in FFmpeg filters:")`, then enumerates `FilterDefinitions` and writes each filter name to stdout as `- {definition.Name}`. The method contains minimal control flow (single `foreach`) and returns the configured command for registration by `Create`.


#### [[DspCommand.CreateFiltersCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateFiltersCommand()
```

**Calls ->**
- [[Log.Info]]

**Called-by <-**
- [[DspCommand.Create]]

