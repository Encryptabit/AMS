---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
  - llm/entry-point
  - llm/factory
---
# DspCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates and returns the top-level DSP CLI command by composing all DSP subcommands into one root command.**

`DspCommand.Create()` builds the root `System.CommandLine.Command` named `dsp` with the description "Audio processing utilities powered by Plugalyzer". It wires the command tree by calling `AddCommand(...)` for 11 subcommand factories (`CreateRunCommand`, `CreateListParamsCommand`, `CreateListPluginsCommand`, `CreateOutputModeCommand`, `CreateOverwriteCommand`, `CreateSetDirCommand`, `CreateInitCommand`, `CreateChainCommand`, `CreateFiltersCommand`, `CreateFilterChainCommand`, `CreateTestAllCommand`) and returns the configured root command. The implementation is a thin composition layer with no branching, I/O, or execution logic.


#### [[DspCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[DspCommand.CreateChainCommand]]
- [[DspCommand.CreateFilterChainCommand]]
- [[DspCommand.CreateFiltersCommand]]
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateListParamsCommand]]
- [[DspCommand.CreateListPluginsCommand]]
- [[DspCommand.CreateOutputModeCommand]]
- [[DspCommand.CreateOverwriteCommand]]
- [[DspCommand.CreateRunCommand]]
- [[DspCommand.CreateSetDirCommand]]
- [[DspCommand.CreateTestAllCommand]]

**Called-by <-**
- [[Program.Main]]

