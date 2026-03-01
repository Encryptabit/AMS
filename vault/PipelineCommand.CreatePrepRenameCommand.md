---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
---
# PipelineCommand::CreatePrepRenameCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


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

