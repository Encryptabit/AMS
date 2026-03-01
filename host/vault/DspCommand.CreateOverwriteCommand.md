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
# DspCommand::CreateOverwriteCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the `dsp overwrite` CLI subcommand that reports or changes the session default for overwriting output files.**

`CreateOverwriteCommand` builds a `System.CommandLine.Command` named `overwrite` with an optional `state` argument defaulting to `null`. Its handler reads `state` from `context.ParseResult`; if absent/whitespace, it prints the current `DspSessionState.OverwriteOutputs` value as `on`/`off` and exits. Otherwise it normalizes input with `Trim().ToLowerInvariant()`, maps `on|true|yes` to `true` and `off|false|no` to `false`, updates `DspSessionState.OverwriteOutputs`, and prints a confirmation message. Any other token triggers `InvalidOperationException("State must be 'on' or 'off'.")` before returning the configured command.


#### [[DspCommand.CreateOverwriteCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateOverwriteCommand()
```

**Called-by <-**
- [[DspCommand.Create]]

