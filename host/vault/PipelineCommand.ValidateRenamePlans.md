---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# PipelineCommand::ValidateRenamePlans
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Pre-validate planned chapter rename operations to prevent filesystem conflicts and duplicate final file destinations.**

`ValidateRenamePlans` materializes all `DirectoryOps` and `FileOps` from the input `ChapterRenamePlan` set and performs preflight collision checks before any rename executes. It skips no-op renames via `PathsEqual`, rejects targets that already exist (`Directory.Exists`/`File.Exists`) by throwing `InvalidOperationException`, and normalizes targets with `Path.GetFullPath` under case-insensitive sets. For files, it sorts directory ops by descending source length and uses `ProjectPath` to project final destinations after directory renames, then detects duplicate final targets and throws if multiple sources converge.


#### [[PipelineCommand.ValidateRenamePlans]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ValidateRenamePlans(IEnumerable<PipelineCommand.ChapterRenamePlan> plans)
```

**Calls ->**
- [[PipelineCommand.PathsEqual]]
- [[PipelineCommand.ProjectPath]]

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

