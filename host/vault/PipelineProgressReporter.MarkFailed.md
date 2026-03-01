---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# PipelineProgressReporter::MarkFailed
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks a chapter’s progress entry as failed and closes the corresponding task in a thread-safe manner.**

`MarkFailed` performs a guarded lookup in `_tasks` via `_tasks.TryGetValue(chapterId, out task)` and returns immediately when the chapter is missing. When found, it acquires `lock (_sync)`, sets `task.Value = PipelineStageCount`, updates `task.Description` to a formatted failure line (`$"{chapterId,-20} [red]Failed[/] {message}"`), and calls `task.StopTask()` to terminate the progress task.


#### [[PipelineProgressReporter.MarkFailed]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkFailed(string chapterId, string message)
```

**Called-by <-**
- [[PipelineCommand.CreateRun]]

