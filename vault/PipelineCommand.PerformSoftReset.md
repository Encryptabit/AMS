---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 3
tags:
  - method
---
# PipelineCommand::PerformSoftReset
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.PerformSoftReset]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PerformSoftReset(DirectoryInfo root, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineCommand.LooksLikeChapterDirectory]]
- [[PipelineCommand.NormalizeDirectoryPath]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreatePrepResetCommand]]

