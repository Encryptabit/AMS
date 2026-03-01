---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
---
# PipelineCommand::EnsureDirectory
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.EnsureDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureDirectory(string dir)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.CreateVerifyCommand]]
- [[PipelineCommand.RunPipelineAsync]]
- [[PipelineCommand.RunVerify]]

