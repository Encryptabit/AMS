---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/utility
---
# BookCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

## Summary
**Build and return the `book` command with its populate-phonemes and verify subcommands attached.**

`BookCommand.Create()` is a static factory method that assembles the CLI `Command` for `Ams.Cli.Commands.BookCommand` by delegating subcommand construction to `CreatePopulatePhonemes()` and `CreateVerify()`. Its reported complexity of 1 indicates straight-line composition logic with no branching, and it is wired into application startup via `Main`.


#### [[BookCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[BookCommand.CreatePopulatePhonemes]]
- [[BookCommand.CreateVerify]]

**Called-by <-**
- [[Program.Main]]

