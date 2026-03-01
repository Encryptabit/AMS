---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# IPipelineProgressReporter::MarkRunning
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Provides a reporter abstraction hook to mark a chapter as currently running in the pipeline lifecycle.**

`IPipelineProgressReporter.MarkRunning(string chapterId)` is a synchronous contract used by `RunPipelineForMultipleChaptersAsync` immediately before `RunPipelineAsync` to signal a chapter’s transition from queued to active work. The interface itself has no logic, but concrete implementations in `PipelineCommand` map this to UI state updates: `PipelineProgressReporter` calls `Update(chapterId, PipelineStage.Pending, "Running...")`, `CompactPipelineProgressReporter` sets `IsRunning = true` (and sets `"In progress..."` when still pending), and `NullProgressReporter` intentionally no-ops.


#### [[IPipelineProgressReporter.MarkRunning]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void MarkRunning(string chapterId)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]

