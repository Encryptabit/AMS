---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 6
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::CreatePrepStageCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Creates and executes a pipeline prep subcommand that stages processed WAV files from a root tree into a batch delivery folder with filtering, naming normalization, and overwrite controls.**

`CreatePrepStageCommand()` constructs the `prep stage` CLI subcommand (System.CommandLine), defines `--root/-r`, `--output/-o`, `--overwrite`, and `--adjusted`, and attaches a synchronous handler. The handler resolves the root via `CommandInputResolver.ResolveDirectory`, validates it exists, ensures the destination directory exists (defaulting to `root/Batch 2`), normalizes destination path, and recursively enumerates either treated or pause-adjusted WAVs while excluding anything already inside the destination using `IsWithinDirectory`. It then copies each file to the destination using `GetStagedFileName()` (which strips `.treated`/`.pause-adjusted` markers), respects cancellation and overwrite behavior, and emits `Log.Debug` messages for empty input, per-file skips, and final staged count.


#### [[PipelineCommand.CreatePrepStageCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePrepStageCommand()
```

**Calls ->**
- [[PipelineCommand.EnsureDirectory]]
- [[PipelineCommand.GetStagedFileName]]
- [[PipelineCommand.IsWithinDirectory]]
- [[PipelineCommand.NormalizeDirectoryPath]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreatePrepCommand]]

