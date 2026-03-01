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
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateSetDirClearCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the CLI subcommand that clears all configured DSP plugin directories, guarded by an explicit `--yes` confirmation check.**

`CreateSetDirClearCommand()` builds a `System.CommandLine.Command` named `"clear"` with description `"Remove all configured plugin directories"` and adds a `--yes` boolean option configured with `ArgumentArity.ZeroOrOne`. Its async handler reads cancellation from `context`, parses confirmation, and if not confirmed logs a debug message, sets `context.ExitCode = 1`, and returns early. When confirmed, it calls `DspConfigService.LoadAsync(token)`, no-ops with a debug log if `PluginDirectories` is empty, otherwise clears the list, persists via `DspConfigService.SaveAsync(config, token)`, and logs completion.


#### [[DspCommand.CreateSetDirClearCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirClearCommand()
```

**Calls ->**
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateSetDirCommand]]

