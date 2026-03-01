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
  - llm/error-handling
---
# IPipelineProgressReporter::MarkFailed
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Records a chapter pipeline failure and associated message in whichever progress reporter implementation is active.**

`IPipelineProgressReporter.MarkFailed` is a failure-reporting contract invoked from `RunPipelineForMultipleChaptersAsync` when worker startup is canceled or pipeline execution throws, typically with `"Cancelled"` or `ex.Message`. In `PipelineProgressReporter`, it `TryGetValue`s the chapter task, locks `_sync`, forces progress to `PipelineStageCount`, sets a red `Failed` description, and calls `StopTask`; `CompactPipelineProgressReporter` sets stage/message/failure flags and refreshes the live status, while `NullProgressReporter` is a no-op.


#### [[IPipelineProgressReporter.MarkFailed]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void MarkFailed(string chapterId, string message)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]

