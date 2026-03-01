---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 7
tags:
  - method
---
# PipelineCommand::ComputeChapterStats
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.ComputeChapterStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PipelineCommand.ChapterStats ComputeChapterStats(DirectoryInfo chapterDir, BookIndex bookIndex, FileInfo bookIndexFile)
```

**Calls ->**
- [[PipelineCommand.ComputeAudioStats]]
- [[PipelineCommand.ExtractChapterStem]]
- [[PipelineCommand.LoadJson]]
- [[PipelineCommand.LoadMfaSilences]]
- [[PausePolicyResolver.Resolve]]
- [[Log.Debug]]
- [[PauseMapBuilder.Build]]

**Called-by <-**
- [[PipelineCommand.RunStats]]

