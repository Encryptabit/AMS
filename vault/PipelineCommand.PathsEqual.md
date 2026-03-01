---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 6
fan_out: 0
tags:
  - method
---
# PipelineCommand::PathsEqual
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.PathsEqual]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool PathsEqual(string left, string right)
```

**Called-by <-**
- [[PipelineCommand.CollectRenameOperations]]
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.ProjectPath]]
- [[PipelineCommand.ResolveRenameTargets]]
- [[PipelineCommand.ResolveVerifyTargets]]
- [[PipelineCommand.ValidateRenamePlans]]

