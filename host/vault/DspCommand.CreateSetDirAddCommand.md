---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/factory
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateSetDirAddCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the CLI `set-dir add` subcommand that validates and persists newly added plugin directory paths to DSP configuration.**

`CreateSetDirAddCommand` constructs a `System.CommandLine` command named `add` with a required `paths` argument (`ArgumentArity.OneOrMore`) and an async handler. The handler loads config via `DspConfigService.LoadAsync`, canonicalizes each input path with `Path.GetFullPath`, validates existence with `Directory.Exists`, and short-circuits on first missing directory by logging `Log.Error` and setting `context.ExitCode = 1`. It de-duplicates additions using case-insensitive comparison (`StringComparer.OrdinalIgnoreCase`), logs a no-op via `Log.Debug` when all inputs are duplicates, otherwise persists via `DspConfigService.SaveAsync` and logs each added directory in case-insensitive sorted order.


#### [[DspCommand.CreateSetDirAddCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirAddCommand()
```

**Calls ->**
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]
- [[Log.Debug]]
- [[Log.Error_2]]

**Called-by <-**
- [[DspCommand.CreateSetDirCommand]]

