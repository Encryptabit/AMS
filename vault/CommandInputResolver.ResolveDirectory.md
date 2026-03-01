---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 3
fan_in: 6
fan_out: 0
tags:
  - method
---
# CommandInputResolver::ResolveDirectory
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`


#### [[CommandInputResolver.ResolveDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static DirectoryInfo ResolveDirectory(DirectoryInfo provided)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.CreatePrepResetCommand]]
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.CreateVerifyCommand]]
- [[PipelineCommand.RunStats]]
- [[ValidateCommand.CreateServeCommand]]

