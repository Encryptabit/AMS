---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
---
# PipelineCommand::CreatePrepResetCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.CreatePrepResetCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreatePrepResetCommand()
```

**Calls ->**
- [[PipelineCommand.PerformHardReset]]
- [[PipelineCommand.PerformSoftReset]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreatePrepCommand]]

