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
  - llm/async
  - llm/data-access
---
# DspCommand::CreateSetDirListCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the `dsp set-dir list` CLI command that asynchronously reads configured plugin directories and prints them to the console.**

`CreateSetDirListCommand` is a command-factory method that builds the `list` subcommand (`new Command("list", "Show configured plugin directories")`) and wires an async handler via `SetHandler`. The handler pulls the cancellation token from `context`, loads persisted DSP config with `await DspConfigService.LoadAsync(token).ConfigureAwait(false)`, and branches on `config.PluginDirectories.Count`. It writes either a guidance message when no directories are configured or emits a header followed by each configured directory line-by-line.


#### [[DspCommand.CreateSetDirListCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirListCommand()
```

**Calls ->**
- [[DspConfigService.LoadAsync]]

**Called-by <-**
- [[DspCommand.CreateSetDirCommand]]

