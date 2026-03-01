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
  - llm/validation
  - llm/error-handling
---
# DspCommand::CreateChainInsertCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Constructs and wires the DSP `insert` command that validates an index and inserts a newly built node into a persisted chain definition.**

`CreateChainInsertCommand` is a command-factory method that builds the `insert` CLI subcommand with a required zero-based `index` argument, node options (`NodeOptionBundle`), and optional `--chain` path. Its async handler resolves the chain file, loads/creates the chain via `LoadChainAsync(..., createIfMissing: true)`, loads DSP config, copies `chain.Nodes` to a mutable list, and validates `index` (`0..nodes.Count`) before insertion. It creates the node from CLI options using `CreateNodeFromOptions(context, options, baseDirectory, config)`, inserts it at the requested index, persists the updated record (`chain with { Nodes = nodes }`) through `SaveChainAsync`, and logs the insertion via `Log.Debug`.


#### [[DspCommand.CreateChainInsertCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainInsertCommand()
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

