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
# DspCommand::CreateFilterChainRunCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates and returns the CLI `run` command that executes a DSP filter chain from either ad-hoc filter names or a persisted filter-chain configuration.**

`CreateFilterChainRunCommand` builds the `run` subcommand and wires options for input/config selection, ad-hoc filter names, persistence (`--save`/`--output`), and log verbosity (`--print-astats`/`--raw`). Its async handler resolves the input audio via `CommandInputResolver.RequireAudio`, then either materializes filter configs from `--filters` (`ResolveFilterDefinitions` + `CreateFilterConfig`) or loads them from disk (`ResolveFilterConfigFile` + `FilterChainConfig.LoadAsync`) and keeps only enabled entries. It enforces argument validity (`--output` requires `--save`, and config-driven runs must have at least one enabled filter), executes `ExecuteFilterChain(...)`, and on any exception logs via `Log.Error` and sets `context.ExitCode = 1` before returning the configured `Command`.


#### [[DspCommand.CreateFilterChainRunCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateFilterChainRunCommand()
```

**Calls ->**
- [[DspCommand.CreateFilterConfig]]
- [[DspCommand.ExecuteFilterChain]]
- [[DspCommand.ResolveFilterConfigFile]]
- [[DspCommand.ResolveFilterDefinitions]]
- [[FilterChainConfig.LoadAsync]]
- [[CommandInputResolver.RequireAudio]]
- [[Log.Error]]
- [[Log.Info]]

**Called-by <-**
- [[DspCommand.CreateFilterChainCommand]]

