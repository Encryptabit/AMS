---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/di
  - llm/validation
---
# ValidateCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Create and compose the validate command (with report/serve/timing subcommands) for the CLI using an injected `ValidationService`.**

`ValidateCommand.Create(ValidationService validationService)` is a static factory that builds and returns the `Command` node for the validate CLI surface, and is invoked from `Main` as part of startup wiring. Its implementation is linear (complexity 1), delegating subcommand construction to `CreateReportCommand`, `CreateServeCommand`, and `CreateTimingCommand`. The provided `ValidationService` is the dependency used to wire validation behavior into the command graph during creation.


#### [[ValidateCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(ValidationService validationService)
```

**Calls ->**
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateServeCommand]]
- [[ValidateCommand.CreateTimingCommand]]

**Called-by <-**
- [[Program.Main]]

