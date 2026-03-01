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
  - llm/utility
---
# DspCommand::CreateSetDirCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates the `set-dir` command group and wires in list/add/remove/clear plugin-directory subcommands.**

`CreateSetDirCommand` builds a `System.CommandLine.Command` named `set-dir` with the description "Manage directories containing plugins". It creates a `root` command and composes the CLI branch by attaching four subcommands via `root.AddCommand(...)`: `CreateSetDirListCommand`, `CreateSetDirAddCommand`, `CreateSetDirRemoveCommand`, and `CreateSetDirClearCommand`. The method contains no handler, validation, async flow, or state mutation; it is a minimal command-factory node invoked from `Create`.


#### [[DspCommand.CreateSetDirCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirCommand()
```

**Calls ->**
- [[DspCommand.CreateSetDirAddCommand]]
- [[DspCommand.CreateSetDirClearCommand]]
- [[DspCommand.CreateSetDirListCommand]]
- [[DspCommand.CreateSetDirRemoveCommand]]

**Called-by <-**
- [[DspCommand.Create]]

