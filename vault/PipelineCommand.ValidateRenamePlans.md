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
# PipelineCommand::ValidateRenamePlans
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.ValidateRenamePlans]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ValidateRenamePlans(IEnumerable<PipelineCommand.ChapterRenamePlan> plans)
```

**Calls ->**
- [[PipelineCommand.PathsEqual]]
- [[PipelineCommand.ProjectPath]]

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

