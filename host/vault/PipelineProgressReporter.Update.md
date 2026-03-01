---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineProgressReporter::Update
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Safely update an existing chapter progress task’s stage value and rendered status text.**

Update is the centralized mutation path used by SetQueued, MarkRunning, ReportStage, and MarkComplete in PipelineProgressReporter. It performs a _tasks.TryGetValue(chapterId, out task) guard and exits early if the chapter task does not exist. For existing tasks, it locks on _sync, clamps the numeric stage with Math.Min((int)stage, PipelineStageCount), writes task.Value, and refreshes task.Description via BuildDescription(chapterId, stage, message).


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

