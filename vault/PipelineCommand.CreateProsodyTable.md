---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineCommand::CreateProsodyTable
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.CreateProsodyTable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Table CreateProsodyTable(PauseStatsSet stats)
```

**Calls ->**
- [[PipelineCommand.EnumerateStats]]

**Called-by <-**
- [[PipelineCommand.PrintStatsReport]]

