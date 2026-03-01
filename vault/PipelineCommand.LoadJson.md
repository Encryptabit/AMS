---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
---
# PipelineCommand::LoadJson
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.LoadJson]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static T LoadJson<T>(FileInfo file)
```

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]
- [[PipelineCommand.RunStats]]

