---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 1
tags:
  - method
---
# PipelineProgressReporter::Update
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineProgressReporter.Update]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void Update(string chapterId, PipelineStage stage, string message)
```

**Calls ->**
- [[PipelineProgressReporter.BuildDescription]]

**Called-by <-**
- [[PipelineProgressReporter.MarkComplete]]
- [[PipelineProgressReporter.MarkRunning]]
- [[PipelineProgressReporter.ReportStage]]
- [[PipelineProgressReporter.SetQueued]]

