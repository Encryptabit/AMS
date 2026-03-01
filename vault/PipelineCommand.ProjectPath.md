---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 2
tags:
  - method
---
# PipelineCommand::ProjectPath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.ProjectPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ProjectPath(string path, IReadOnlyList<PipelineCommand.RenameOp> directoryOps)
```

**Calls ->**
- [[PipelineCommand.EnsureTrailingSeparator]]
- [[PipelineCommand.PathsEqual]]

**Called-by <-**
- [[PipelineCommand.ValidateRenamePlans]]

