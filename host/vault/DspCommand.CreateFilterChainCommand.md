---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/factory
---
# DspCommand::CreateFilterChainCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Build and return the `dsp filter-chain` CLI command node that groups the `init` and `run` filter-chain workflows.**

`CreateFilterChainCommand` is a thin `System.CommandLine` composition method that instantiates `new Command("filter-chain", "Manage FFmpeg filter chains")`, attaches `CreateFilterChainInitCommand()` and `CreateFilterChainRunCommand()` as subcommands, and returns the parent command. It has no direct option binding or handler logic, delegating execution behavior entirely to those two child command builders.


#### [[DspCommand.CreateFilterChainCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateFilterChainCommand()
```

**Calls ->**
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterChainRunCommand]]

**Called-by <-**
- [[DspCommand.Create]]

