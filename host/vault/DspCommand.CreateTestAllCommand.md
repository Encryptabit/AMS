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
  - llm/entry-point
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateTestAllCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the `dsp test-all` command that runs the complete built-in FFmpeg filter chain on an audio input with optional output persistence and raw log printing.**

`CreateTestAllCommand` builds a `System.CommandLine.Command` named `test-all`, registers `--input/-i`, `--save`, `--output`, and `--raw`, and attaches a synchronous handler. The handler resolves audio with `CommandInputResolver.RequireAudio`, creates configs for every built-in filter via `FilterDefinitions.Select(CreateFilterConfig).ToList()`, enforces that `--output` is only allowed when `--save` is set, and invokes `ExecuteFilterChain(inputFile, filterConfigs, save, outputFile, printAstats: false, printRaw: printRaw)`. Exceptions are caught, logged with `Log.Error(..., "dsp test-all failed")`, and converted to a non-zero CLI exit via `context.ExitCode = 1`.


#### [[DspCommand.CreateTestAllCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateTestAllCommand()
```

**Calls ->**
- [[DspCommand.CreateFilterConfig]]
- [[DspCommand.ExecuteFilterChain]]
- [[CommandInputResolver.RequireAudio]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

