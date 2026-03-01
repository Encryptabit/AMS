---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 9
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateRunCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the `dsp run` CLI subcommand and wires end-to-end option parsing, chain resolution/building, execution, and error handling for DSP processing.**

`CreateRunCommand` is a command factory that builds `new Command("run", ...)`, registers all DSP-run options (`--input/--output/--chain/--plugin/--param/...`) and attaches an async `SetHandler` that pulls option values from `context.ParseResult`. The handler resolves required audio input (`CommandInputResolver.RequireAudio`), derives output (`ResolveOutputFile`), tries an explicit or default chain file (`ResolveChainFile`, with debug logging when auto-selected), and enforces that either `--chain` or `--plugin` is provided. It then either loads and validates a JSON chain (`LoadChainAsync`, rejects zero-node chains) or creates a single-node chain (`DspConfigService.LoadAsync` + `BuildSingleNodeChain`), and runs processing through `RunChainAsync` with override/work/temp/overwrite settings. Failures are centrally handled with `Log.Error(...)` and `context.ExitCode = 1`.


#### [[DspCommand.CreateRunCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateRunCommand()
```

**Calls ->**
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.LoadChainAsync]]
- [[DspCommand.ResolveChainFile]]
- [[DspCommand.ResolveOutputFile]]
- [[DspCommand.RunChainAsync]]
- [[DspConfigService.LoadAsync]]
- [[CommandInputResolver.RequireAudio]]
- [[Log.Debug]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

