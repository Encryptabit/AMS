---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineCommand::ResolveRenameTargets
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.ResolveRenameTargets]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<FileInfo> ResolveRenameTargets(DirectoryInfo root, bool forceAll = false)
```

**Calls ->**
- [[PipelineCommand.PathsEqual]]

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

