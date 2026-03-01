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
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/data-access
  - llm/validation
---
# DspCommand::CreateListPluginsCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates and wires the CLI command that lists cached DSP plugins, optionally filtered and optionally showing extended metadata.**

`CreateListPluginsCommand` is a command factory that builds the `dsp list` CLI command with `--filter` and `--detailed` options, then attaches an async handler. The handler loads cached plugin metadata via `DspConfigService.LoadAsync(token)`, short-circuits with user-facing messages when the cache is empty or filtering yields no matches, and applies case-insensitive substring filtering against both `PluginName` and the dictionary key/path. Results are sorted by display name fallback (`PluginName ?? Path.GetFileName(key)`) and path, then printed with optional scan/modification timestamps and parameter counts when `--detailed` is set.


#### [[DspCommand.CreateListPluginsCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateListPluginsCommand()
```

**Calls ->**
- [[DspConfigService.LoadAsync]]

**Called-by <-**
- [[DspCommand.Create]]

