---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 6
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/async
  - llm/factory
---
# DspCommand::CreateChainAddCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates and returns the CLI command that appends a newly configured DSP node to a chain definition file.**

`CreateChainAddCommand` builds a `System.CommandLine` subcommand named `add` (alias `append`), attaches node options through `NodeOptionBundle`, and adds an optional `--chain` file argument. Its async handler resolves the target chain file, loads/creates the chain via `LoadChainAsync(..., createIfMissing: true)`, loads DSP config via `DspConfigService.LoadAsync`, and creates a node with `CreateNodeFromOptions` using the chain file directory as base path. It appends the node by copying `chain.Nodes` to a list, reconstructs the record with `chain with { Nodes = nodes }`, saves via `SaveChainAsync`, and logs a debug message with a name fallback to `Path.GetFileNameWithoutExtension(node.Plugin)`.


#### [[DspCommand.CreateChainAddCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainAddCommand()
```

**Calls ->**
- [[DspCommand.CreateNodeFromOptions]]
- [[DspCommand.LoadChainAsync]]
- [[DspCommand.ResolveChainFile]]
- [[DspCommand.SaveChainAsync]]
- [[DspConfigService.LoadAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateChainCommand]]

