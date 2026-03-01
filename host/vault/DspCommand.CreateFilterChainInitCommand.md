---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateFilterChainInitCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Builds the CLI subcommand that initializes or overwrites a filter-chain JSON config using selected built-in filters and their default parameters.**

CreateFilterChainInitCommand constructs the `dsp filter-chain init` `System.CommandLine.Command`, registering `--filters` (multi-arg, defaults empty) and `--config` (optional file path) options. Its async handler reads the cancellation token from the invocation context, resolves the config destination (`ResolveFilterConfigFile`) and selected filters (`ResolveFilterDefinitions`), then creates a `FilterChainConfig` by adding enabled `FilterConfig` entries with serialized defaults via `SerializeParameters(definition.DefaultParameters ?? CreateDefaultParameterInstance(definition))`. It writes the config with `SaveAsync`, logs success with `Log.Info`, and wraps execution in a catch-all that logs `Log.Error` and sets `context.ExitCode = 1`.


#### [[DspCommand.CreateFilterChainInitCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateFilterChainInitCommand()
```

**Calls ->**
- [[DspCommand.CreateDefaultParameterInstance]]
- [[DspCommand.ResolveFilterConfigFile]]
- [[DspCommand.ResolveFilterDefinitions]]
- [[DspCommand.SerializeParameters]]
- [[FilterChainConfig.SaveAsync]]
- [[Log.Error]]
- [[Log.Info]]

**Called-by <-**
- [[DspCommand.CreateFilterChainCommand]]

