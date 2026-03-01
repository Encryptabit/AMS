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
  - llm/factory
  - llm/utility
---
# DspCommand::CreateChainCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates and returns the CLI command group for interactive DSP chain management.**

This method constructs the `chain` command node for `System.CommandLine` by creating `new Command("chain", "Interactively manage DSP chain files")` and attaching child commands with `AddCommand(...)`. It wires the chain subcommands in a fixed sequence (`list`, `add`, `prepend`, `insert`, `remove`) via `CreateChain*Command` helpers, then returns the assembled `root` command. Control flow is linear with no validation, async work, or error handling.


#### [[DspCommand.CreateChainCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainCommand()
```

**Calls ->**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateChainPrependCommand]]
- [[DspCommand.CreateChainRemoveCommand]]

**Called-by <-**
- [[DspCommand.Create]]

