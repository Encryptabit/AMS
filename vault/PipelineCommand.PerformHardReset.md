---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineCommand::PerformHardReset
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.PerformHardReset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PerformHardReset(DirectoryInfo root, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreatePrepResetCommand]]

