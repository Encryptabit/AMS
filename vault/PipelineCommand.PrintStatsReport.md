---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 3
tags:
  - method
---
# PipelineCommand::PrintStatsReport
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.PrintStatsReport]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PrintStatsReport(DirectoryInfo root, FileInfo bookIndexFile, BookIndex bookIndex, IReadOnlyList<PipelineCommand.ChapterStats> chapters, double totalAudioSec)
```

**Calls ->**
- [[PipelineCommand.CreateAudioTable]]
- [[PipelineCommand.CreateProsodyTable]]
- [[PipelineCommand.FormatDuration]]

**Called-by <-**
- [[PipelineCommand.RunStats]]

