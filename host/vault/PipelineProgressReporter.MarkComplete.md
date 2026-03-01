---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# PipelineProgressReporter::MarkComplete
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks a chapter as complete in the pipeline progress UI and stops its associated progress task safely.**

`MarkComplete` transitions a chapter’s progress task to done by calling `Update(chapterId, PipelineStage.Complete, "Complete")`, which clamps/stores stage state under `_sync` and refreshes the task description. It then performs a second `_tasks.TryGetValue` and, if found, acquires `_sync` again to call `task.StopTask()`. Missing `chapterId` entries are handled defensively through early-return guards, so unknown chapters are ignored without throwing.


#### [[PipelineProgressReporter.MarkComplete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkComplete(string chapterId)
```

**Calls ->**
- [[PipelineProgressReporter.Update]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]

