---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# PipelineProgressReporter::SetQueued
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks a chapter’s progress task as queued (pending) in the pipeline progress reporter.**

`SetQueued` in `PipelineProgressReporter` is a one-line state transition that delegates to `Update(chapterId, PipelineStage.Pending, "Queued")`. The actual mutation path in `Update` first checks `_tasks.TryGetValue(chapterId, out task)` and silently returns if missing; otherwise it enters `lock (_sync)`, sets `task.Value` (clamped stage ordinal), and rewrites `task.Description` using `BuildDescription`. This keeps queue-state updates centralized and thread-safe for the chapter task created at reporter initialization.


#### [[PipelineProgressReporter.SetQueued]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetQueued(string chapterId)
```

**Calls ->**
- [[PipelineProgressReporter.Update]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]

