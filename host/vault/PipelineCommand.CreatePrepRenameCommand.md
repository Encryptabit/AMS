---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::CreatePrepRenameCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Creates the CLI command that safely renames chapter audio/artifact files and directories from a naming pattern, with dry-run preview and post-rename REPL refresh.**

`CreatePrepRenameCommand` builds the `pipeline prep rename` `System.CommandLine` subcommand, wiring `--root/-r`, required `--pattern/-p`, `--dry-run`, and `--all` options plus a synchronous handler. The handler resolves and verifies the root directory (`ResolveDirectory`), enumerates target chapters (`ResolveRenameTargets`), derives new stems via `ExtractUnmatchedParts` + `ApplyRenamePattern`, rejects empty/invalid/duplicate outputs, skips unchanged names, and constructs rename plans with `BuildRenamePlan`. It validates cross-plan safety with `ValidateRenamePlans`, logs planned operations in dry-run mode, otherwise executes file moves first then directory moves (descending source-path length), and finally refreshes REPL chapter state when the renamed root matches the active working directory (`PathsEqual`/`RefreshChapters`). Error paths log with `Log.Error` and set `context.ExitCode = 1` for invalid input or caught exceptions.


#### [[PipelineCommand.CreatePrepRenameCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePrepRenameCommand()
```

**Calls ->**
- [[PipelineCommand.ApplyRenamePattern]]
- [[PipelineCommand.BuildRenamePlan]]
- [[PipelineCommand.ExtractUnmatchedParts]]
- [[PipelineCommand.PathsEqual]]
- [[PipelineCommand.ResolveRenameTargets]]
- [[PipelineCommand.ValidateRenamePlans]]
- [[ReplState.RefreshChapters]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]

**Called-by <-**
- [[PipelineCommand.CreatePrepCommand]]

