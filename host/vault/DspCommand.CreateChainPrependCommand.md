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
  - llm/factory
  - llm/async
  - llm/data-access
---
# DspCommand::CreateChainPrependCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Builds the CLI command that prepends a new DSP node to the start of a persisted chain definition.**

`CreateChainPrependCommand` is a command factory for `System.CommandLine` that constructs a `prepend` subcommand, attaches node-definition options via `NodeOptionBundle`, and adds an optional `--chain` file option. The async handler resolves the chain file, loads/creates the chain (`LoadChainAsync(..., createIfMissing: true)`), loads DSP config (`DspConfigService.LoadAsync`), materializes a node from CLI options (`CreateNodeFromOptions`), inserts it at index `0`, saves the updated chain (`SaveChainAsync`), and logs the operation with `Log.Debug`.


#### [[DspCommand.CreateChainPrependCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainPrependCommand()
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

