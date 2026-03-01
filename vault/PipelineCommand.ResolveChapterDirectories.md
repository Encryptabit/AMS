---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineCommand::ResolveChapterDirectories
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.ResolveChapterDirectories]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<DirectoryInfo> ResolveChapterDirectories(DirectoryInfo root, string chapterName, bool analyzeAll)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.RunStats]]

