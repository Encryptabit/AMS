---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 3
tags:
  - method
---
# PipelineCommand::CollectRenameOperations
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


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

