---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/async
  - llm/factory
---
# DspCommand::CreateChainListCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the CLI command that lists chain nodes from the chain file with resolved plugin and human-friendly node details.**

`CreateChainListCommand` is a command factory that constructs the `list` subcommand with a `--chain` `Option<FileInfo?>` and an async handler. The handler resolves the chain file (`ResolveChainFile`), loads the chain with auto-create semantics (`LoadChainAsync(..., createIfMissing: true)`), and loads DSP config (`DspConfigService.LoadAsync`). It prints an empty-chain guidance message when `chain.Nodes.Count == 0`; otherwise it iterates nodes, resolves each plugin path (`ResolvePath`), computes a display name (`node.Name` first, then `TryGetFriendlyName`, then plugin filename), and prints optional description and parameter metadata.


#### [[DspCommand.CreateChainListCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainListCommand()
```

**Calls ->**
- [[DspCommand.LoadChainAsync]]
- [[DspCommand.ResolveChainFile]]
- [[DspCommand.ResolvePath]]
- [[DspCommand.TryGetFriendlyName]]
- [[DspConfigService.LoadAsync]]

**Called-by <-**
- [[DspCommand.CreateChainCommand]]

