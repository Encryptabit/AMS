---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 2
tags:
  - method
---
# PipelineCommand::ResolveVerifyTargets
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.ResolveVerifyTargets]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<FileInfo> ResolveVerifyTargets(DirectoryInfo root, string chapterName, bool verifyAll)
```

**Calls ->**
- [[PipelineCommand.PathsEqual]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

