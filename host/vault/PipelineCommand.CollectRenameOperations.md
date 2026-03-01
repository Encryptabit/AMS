---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/data-access
  - llm/utility
---
# PipelineCommand::CollectRenameOperations
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Recursively collects directory and file rename operations for a tree when switching from one naming stem to another.**

`CollectRenameOperations` does a depth-first traversal from `currentDirOldPath` using `Directory.EnumerateDirectories` and `Directory.EnumerateFiles`, building rename plans instead of executing renames. For each subdirectory, it computes a renamed segment with `ReplaceStem`, projects the destination under `currentDirNewPath`, records a `RenameOp` when `PathsEqual` says source/target differ, then recurses with the child old/new directory pair. For files, it applies the same stem replacement to file names, targets them within `currentDirOldPath`, and records only non-no-op operations, relying on normalized full-path comparison in `PathsEqual`.


#### [[PipelineCommand.CollectRenameOperations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CollectRenameOperations(string root, string currentDirOldPath, string currentDirNewPath, string oldStem, string newStem, List<PipelineCommand.RenameOp> directoryOps, List<PipelineCommand.RenameOp> fileOps)
```

**Calls ->**
- [[PipelineCommand.CollectRenameOperations]]
- [[PipelineCommand.PathsEqual]]
- [[PipelineCommand.ReplaceStem]]

**Called-by <-**
- [[PipelineCommand.BuildRenamePlan]]
- [[PipelineCommand.CollectRenameOperations]]

