---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 14
fan_in: 1
fan_out: 7
tags:
  - method
---
# PipelineCommand::RunStats
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.RunStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void RunStats(DirectoryInfo workDirOption, FileInfo bookIndexOption, string chapterName, bool analyzeAll, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineCommand.ComputeChapterStats]]
- [[PipelineCommand.LoadJson]]
- [[PipelineCommand.PrintStatsReport]]
- [[PipelineCommand.ResolveChapterDirectories]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]
- [[Log.Error_2]]

**Called-by <-**
- [[PipelineCommand.CreateStatsCommand]]

