---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineCommand::LogStageInfo
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.LogStageInfo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void LogStageInfo(bool quiet, string message, params object[] args)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.RunPipelineAsync]]

