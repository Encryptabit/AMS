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
  - llm/utility
  - llm/di
---
# IPipelineProgressReporter::SetQueued
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks the given chapter ID as queued in the injected pipeline progress reporting backend.**

IPipelineProgressReporter.SetQueued is the contract hook for putting a chapter into the pending/queued progress state. RunPipelineForMultipleChaptersAsync invokes reporter?.SetQueued(chapterId) before waiting on the worker semaphore, so chapters appear queued before processing starts. Implementations map this consistently: PipelineProgressReporter delegates to Update(chapterId, PipelineStage.Pending, "Queued"), CompactPipelineProgressReporter resets queued fields under a lock with live UI refresh, and NullProgressReporter is a no-op.


#### [[IPipelineProgressReporter.SetQueued]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SetQueued(string chapterId)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]

