---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::BuildRenamePlan
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Constructs a chapter-scoped rename execution plan by discovering all stem-based directory and file rename operations under the chapter folder.**

`BuildRenamePlan` derives the chapter root path from `chapter.Directory?.FullName` and immediately fails with `InvalidOperationException` if the directory cannot be resolved. It initializes separate `List<RenameOp>` collections for directory and file renames, then calls `CollectRenameOperations(root, root, root, oldStem, newStem, directoryOps, fileOps)` to recursively populate both operation sets. It returns a new `ChapterRenamePlan` containing the original `FileInfo`, old/new stems, and the collected operations.


#### [[PipelineCommand.BuildRenamePlan]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PipelineCommand.ChapterRenamePlan BuildRenamePlan(FileInfo chapter, string oldStem, string newStem)
```

**Calls ->**
- [[PipelineCommand.CollectRenameOperations]]

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

