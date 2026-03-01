---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::CreateVerifyCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Creates and configures the pipeline `verify` command, validates/normalizes user inputs, and dispatches verification execution with consistent failure handling.**

`CreateVerifyCommand` builds the `verify` CLI subcommand in `PipelineCommand` by instantiating `Command("verify", ...)`, defining options for root/chapter selection, report output, format, and verification timing thresholds, and wiring them via `AddOption`. The handler resolves `--root` with `CommandInputResolver.ResolveDirectory`, checks directory existence, optionally ensures `--report-dir` exists via `EnsureDirectory`, normalizes/parses `--format` into `VerificationReportFormat` with a guarded switch, then calls `RunVerify(...)` with parsed values and `CancellationToken`. It wraps execution in `try/catch` to log cancellation (`Debug`) or failures (`Error`) and sets `context.ExitCode = 1` on error paths.


#### [[PipelineCommand.CreateVerifyCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateVerifyCommand()
```

**Calls ->**
- [[PipelineCommand.EnsureDirectory]]
- [[PipelineCommand.RunVerify]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]
- [[Log.Error]]

**Called-by <-**
- [[PipelineCommand.Create]]

