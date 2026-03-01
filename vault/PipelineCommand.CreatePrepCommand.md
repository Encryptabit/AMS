---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# PipelineCommand::CreatePrepCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.CreatePrepCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePrepCommand()
```

**Calls ->**
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.CreatePrepResetCommand]]
- [[PipelineCommand.CreatePrepStageCommand]]

**Called-by <-**
- [[PipelineCommand.Create]]

