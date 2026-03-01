---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 8
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateInitCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the CLI initialization command that scans VST3 plugins and caches their parameter metadata in persistent DSP config.**

`CreateInitCommand` builds the `dsp init` `Command` with `--plugin` and `--force`, then attaches an async handler that loads config (`DspConfigService.LoadAsync`), resolves scan targets from either one plugin file or configured directories, and prunes stale cached plugin entries no longer discovered on disk. The handler skips unchanged plugins unless forced (`existing.PluginModifiedUtc >= File.GetLastWriteTimeUtc`), runs `PlugalyzerService.RunAsync` with `listParameters`, logs stderr on non-zero exit codes, and on success creates `DspPluginMetadata` using `ExtractPluginName` and `ParseParameterLines` before updating `config.Plugins`. It saves the updated config (`SaveAsync`), logs scanned/skipped/failed counts, and wraps execution in a catch-all that logs and sets `context.ExitCode = 1`.


#### [[DspCommand.CreateInitCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateInitCommand()
```

**Calls ->**
- [[DspCommand.ExtractPluginName]]
- [[DspCommand.ParseParameterLines]]
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]
- [[PlugalyzerService.RunAsync]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

