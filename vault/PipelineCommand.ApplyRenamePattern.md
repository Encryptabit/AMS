---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# PipelineCommand::ApplyRenamePattern
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.ApplyRenamePattern]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ApplyRenamePattern(string pattern, int index, IReadOnlyList<string> unmatchedParts)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

