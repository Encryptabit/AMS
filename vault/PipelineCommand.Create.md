---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
---
# PipelineCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(PipelineService pipelineService)
```

**Calls ->**
- [[PipelineCommand.CreatePrepCommand]]
- [[PipelineCommand.CreateRun]]
- [[PipelineCommand.CreateStatsCommand]]
- [[PipelineCommand.CreateVerifyCommand]]

**Called-by <-**
- [[Program.Main]]

