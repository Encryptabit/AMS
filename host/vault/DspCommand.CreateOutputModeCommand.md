---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateOutputModeCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the CLI command that queries or updates DSP output routing mode in session state.**

`CreateOutputModeCommand` constructs a `System.CommandLine` subcommand named `output-mode` with an optional `mode` argument (`string?`, default `null`) and registers a synchronous handler. The handler reads the argument from `context.ParseResult`, prints the current `DspSessionState.OutputMode` when no value is provided, or normalizes input with `Trim().ToLowerInvariant()` and switches only on `source`/`post` to update `DspSessionState.OutputMode` (`DspOutputMode.Source`/`Post`). Invalid input is rejected by throwing `InvalidOperationException`.


#### [[DspCommand.CreateOutputModeCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateOutputModeCommand()
```

**Called-by <-**
- [[DspCommand.Create]]

