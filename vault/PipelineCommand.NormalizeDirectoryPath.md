---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
---
# PipelineCommand::NormalizeDirectoryPath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.NormalizeDirectoryPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeDirectoryPath(string path)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.PerformSoftReset]]

