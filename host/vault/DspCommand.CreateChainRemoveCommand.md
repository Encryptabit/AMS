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
# DspCommand::CreateChainRemoveCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Create the CLI command that removes a treatment-chain node by index or by name and saves the updated chain file.**

`CreateChainRemoveCommand` builds a System.CommandLine `remove` subcommand with `--chain`, `--index`, and `--name` options, then wires an async handler that resolves the chain path, loads the chain (`createIfMissing: true`), and exits early if `Nodes` is empty with a debug log. The handler enforces that either index or name is provided, validates index bounds, or performs case-insensitive name matching against both `TreatmentNode.Name` and `Path.GetFileNameWithoutExtension(node.Plugin)`, throwing `InvalidOperationException`/`ArgumentOutOfRangeException` on invalid input. On success it removes the matched node from a copied list, creates an updated record via `chain with { Nodes = nodes }`, persists with `SaveChainAsync`, and logs the removed node.


#### [[DspCommand.CreateChainRemoveCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainRemoveCommand()
```

**Calls ->**
- [[DspCommand.LoadChainAsync]]
- [[DspCommand.ResolveChainFile]]
- [[DspCommand.SaveChainAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateChainCommand]]

