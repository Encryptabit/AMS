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
  - llm/validation
---
# PipelineProgressReporter::MarkRunning
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks a chapter’s progress entry as running by updating its displayed stage/message in the shared progress reporter.**

`MarkRunning` in `PipelineCommand.PipelineProgressReporter` is a thin wrapper that calls `Update(chapterId, PipelineStage.Pending, "Running...")` to transition a chapter task into a running state. The delegated `Update` path looks up the chapter in `_tasks`, returns early if missing, and under `_sync` lock updates `ProgressTask.Value` and `Description` (via `BuildDescription`) for Spectre.Console progress rendering. Its cyclomatic complexity is 1 because it contains only a single direct delegate call.


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

