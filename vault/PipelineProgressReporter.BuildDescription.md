---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
---
# PipelineProgressReporter::BuildDescription
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineProgressReporter.BuildDescription]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildDescription(string chapterId, PipelineStage stage, string message)
```

**Called-by <-**
- [[PipelineProgressReporter..ctor]]
- [[PipelineProgressReporter.Update]]

