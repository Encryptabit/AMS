---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::PerformSoftReset
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Performs a non-destructive reset by removing only chapter-like generated directories under a root while preserving batch/CRX directories and honoring cancellation.**

`PerformSoftReset` computes a normalized path for the default batch folder (`root + DefaultBatchFolderName`) and then iterates only top-level subdirectories of `root`, checking `cancellationToken.ThrowIfCancellationRequested()` on each pass. It skips the batch folder, skips `CrxDirectoryName`, and only targets directories that satisfy `LooksLikeChapterDirectory` (artifact-pattern heuristic). Each candidate is deleted recursively in a per-directory `try/catch` that logs failures via `Log.Debug`, while successful deletes increment a counter that is reported in a final debug message.


#### [[PipelineCommand.PerformSoftReset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PerformSoftReset(DirectoryInfo root, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineCommand.LooksLikeChapterDirectory]]
- [[PipelineCommand.NormalizeDirectoryPath]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreatePrepResetCommand]]

