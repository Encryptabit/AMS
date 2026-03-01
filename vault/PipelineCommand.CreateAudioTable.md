---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
---
# PipelineCommand::CreateAudioTable
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.CreateAudioTable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Table CreateAudioTable(PipelineCommand.AudioStats audio)
```

**Calls ->**
- [[PipelineCommand.FormatDb]]
- [[PipelineCommand.FormatDuration]]

**Called-by <-**
- [[PipelineCommand.PrintStatsReport]]

