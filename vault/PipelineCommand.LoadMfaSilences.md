---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# PipelineCommand::LoadMfaSilences
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.LoadMfaSilences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<(double Start, double End)> LoadMfaSilences(FileInfo textGridFile)
```

**Calls ->**
- [[PipelineCommand.IsSilenceLabel]]
- [[Log.Debug]]
- [[TextGridParser.ParseWordIntervals]]

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]

