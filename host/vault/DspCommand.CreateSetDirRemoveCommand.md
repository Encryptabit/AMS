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
  - llm/factory
  - llm/async
  - llm/data-access
---
# DspCommand::CreateSetDirRemoveCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the CLI subcommand that removes one or more plugin directory paths from DSP configuration and saves the updated config when changes occur.**

`CreateSetDirRemoveCommand` builds the `remove` `System.CommandLine.Command` for `dsp set-dir`, with a required `paths` argument (`ArgumentArity.OneOrMore`) and an async handler. The handler reads cancellation token and argument values from `InvocationContext`, loads persisted DSP config via `DspConfigService.LoadAsync`, normalizes each input using `Path.GetFullPath`, and removes matching entries from `config.PluginDirectories` using case-insensitive comparison (`StringComparison.OrdinalIgnoreCase` via `RemoveAll`). If nothing is removed it exits after `Log.Debug`; otherwise it persists changes with `DspConfigService.SaveAsync` and logs each removed directory in case-insensitive sorted order.


#### [[DspCommand.CreateSetDirRemoveCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirRemoveCommand()
```

**Calls ->**
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateSetDirCommand]]

