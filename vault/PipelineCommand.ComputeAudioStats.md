---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 18
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# PipelineCommand::ComputeAudioStats
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

> [!danger] High Complexity (18)
> Cyclomatic complexity: 18. Consider refactoring into smaller methods.


#### [[PipelineCommand.ComputeAudioStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PipelineCommand.AudioStats ComputeAudioStats(FileInfo audioFile)
```

**Calls ->**
- [[AudioProcessor.Decode]]

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]

