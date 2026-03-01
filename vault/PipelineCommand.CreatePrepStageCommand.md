---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 6
tags:
  - method
---
# PipelineCommand::CreatePrepStageCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


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

