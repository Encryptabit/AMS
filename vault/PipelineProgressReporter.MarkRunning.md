---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineProgressReporter::MarkRunning
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineProgressReporter.MarkRunning]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkRunning(string chapterId)
```

**Calls ->**
- [[PipelineProgressReporter.Update]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]

